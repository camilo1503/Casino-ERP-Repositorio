/* =====================================================================
   Dueño: Felipe · Contrato C2 (desarrollo/contratos.md).
   Ver desarrollo/planes/Plan-Felipe.md (F6): empleados (carga 15) y nómina (carga 80).
   Usa window.PitBossApi y los globales del mockup; no edites index.html.

   - Competencias / Turnos: S.staff se carga desde GET /api/empleados conservando los id s1…s18
     (los usan S.users[].sid y table.dealer) y la certificación del prototipo.
   - Registrar empleado: FORM.employee también lo guarda en POST /api/empleados.
   - Turnos y nómina: payroll() y el modal "Nómina de octubre" usan la nómina real
     (POST /api/nomina/calculate al abrir el modal; aprobar con POST /api/nomina/{id}/aprobar).
   Sin API (modo demostración) todo sigue funcionando con los datos de ejemplo del mockup.
   ===================================================================== */
(function () {
  const { pedir, pedirSi, conectar, registrarCarga, util } = PitBossApi;
  const { docApi } = util;

  const ANIO = 2026, MES = 10;
  const CARGO_FRONT = { CRUPIER: 'Crupier', CAJERO: 'Cajero', JEFE_DE_PISO: 'Jefe de piso' };
  const CARGO_API = { Crupier: 'CRUPIER', Cajero: 'CAJERO', 'Jefe de piso': 'JEFE_DE_PISO' };
  const TURNO_FRONT = { MANANA: 'Mañana', TARDE: 'Tarde', NOCHE: 'Noche' };
  const CONTRATO_FRONT = { INDEFINIDO: 'Indefinido', TERMINO_FIJO: 'Término fijo', PRESTACION_SERVICIOS: 'Servicios' };

  /* ---------- 1. Carga "empleados" (orden 15) ---------- */
  registrarCarga('empleados', 15,
    () => pedirSi(['competencias', 'rrhh', 'mesas'], 'GET', '/empleados?soloActivos=true'),
    lista => {
      const previos = S.staff;
      S.staff = lista.map(e => {
        const s = previos.find(x => docApi(x.doc) === e.documento)
          || { id: 'se' + e.id, last: null, score: null, exp: null, revoked: null };
        return Object.assign(s, {
          name: e.nombre,
          role: CARGO_FRONT[e.cargo],
          game: e.juego,
          shift: TURNO_FRONT[e.turno],
          area: e.area,
          doc: grp(+e.documento),
          apiDoc: e.documento,
          contrato: e.tipoContrato,
          salario: e.salarioBase,
        });
      });
    });

  /* ---------- 2. Registrar empleado (Competencias → + Registrar empleado) ---------- */
  conectar(FORM, 'employee', {
    antes: fd => ({
      n: S.staff.length,
      documento: docApi(fd.get('doc')),
      nombre: fd.get('name').trim(),
      cargo: CARGO_API[fd.get('role')],
      juego: fd.get('game'),
    }),
    exito: ctx => S.staff.length > ctx.n,
    guardar: ctx => pedir('POST', '/empleados', {
      documento: ctx.documento,
      nombre: ctx.nombre,
      cargo: ctx.cargo,
      juego: ctx.cargo === 'CRUPIER' ? ctx.juego : null,
      tipoContrato: 'INDEFINIDO',
    }),
  });

  /* ---------- 3. Carga "nomina" (orden 80) ---------- */
  registrarCarga('nomina', 80,
    () => pedirSi('rrhh', 'GET', '/nomina'),
    lista => { S.nomina = lista.find(n => n.anio === ANIO && n.mes === MES) || null; });

  /* ---------- 4. payroll(): la nómina real cuando existe (la usa V.rrhh) ---------- */
  const payrollLocal = payroll;
  payroll = function () {
    if (!PitBossApi.estado.online || !S.nomina) return payrollLocal();
    return S.nomina.detalle.map(d => ({
      s: S.staff.find(x => x.apiDoc === d.empleadoDocumento) || { name: d.empleadoNombre, shift: TURNO_FRONT[d.turno] },
      base: d.salarioBase,
      noct: d.recargoNocturno,
      aux: d.auxilioTransporte,
      ded: d.totalDeducciones,
      net: d.neto,
      contrato: d.tipoContrato,
    }));
  };

  /* ---------- 5. Calcular al abrir el modal de nómina ---------- */
  conectar(ACT, 'modal', {
    antes: a => a === 'payroll' && (!S.nomina || S.nomina.estado === 'BORRADOR') ? {} : null,
    guardar: () => pedir('POST', '/nomina/calculate', { anio: ANIO, mes: MES }),
  });

  /* ---------- 6. Modal "Nómina de octubre · cálculo" con el detalle real ---------- */
  const modalLocal = MODALS.payroll;
  MODALS.payroll = m => {
    if (!PitBossApi.estado.online || !S.nomina) return modalLocal(m);
    const n = S.nomina, filas = payroll(), aprobada = n.estado === 'APROBADA';
    return `<h2>Nómina de octubre · cálculo ${aprobada ? B('Aprobada', 'ok') : ''}</h2>`
      + `<p class="sub">HU-04 · Salario base, recargo nocturno del 35 %, auxilio de transporte (hasta 2 SMMLV) y aportes a salud y pensión (4 % + 4 %) o retención del 10 % en prestación de servicios. Parámetros 2026.</p>`
      + `<div class="tblwrap"><table class="tbl"><thead><tr><th>Empleado</th><th>Turno</th><th>Contrato</th><th class="r">Base</th><th class="r">Recargo</th><th class="r">Auxilio</th><th class="r">Deducciones</th><th class="r">Neto</th></tr></thead><tbody>`
      + filas.map(r => `<tr><td class="b">${esc(r.s.name)}</td><td class="m">${esc(r.s.shift || '')}</td><td>${CONTRATO_FRONT[r.contrato] || ''}</td>`
        + `<td class="r">${money(r.base)}</td><td class="r">${r.noct ? money(r.noct) : '—'}</td><td class="r">${r.aux ? money(r.aux) : '—'}</td>`
        + `<td class="r">-${money(r.ded)}</td><td class="r b">${money(r.net)}</td></tr>`).join('')
      + `<tr><td class="b" colspan="7">Total neto</td><td class="r b">${money(n.totalNeto)}</td></tr>`
      + `<tr><td colspan="7">Aportes del empleador (parafiscales)</td><td class="r">${money(n.totalAportesEmpleador)}</td></tr>`
      + `</tbody></table></div>`
      + `<div class="mfoot"><button class="btn sec" data-act="closeModal">Cerrar</button>`
      + (aprobada ? '' : `<button class="btn primary" data-act="approvePayroll">Aprobar nómina</button>`)
      + `</div>`;
  };

  /* ---------- 7. Aprobar nómina ---------- */
  conectar(ACT, 'approvePayroll', {
    antes: () => S.nomina && S.nomina.estado === 'BORRADOR' ? { id: S.nomina.id } : null,
    guardar: ({ id }) => pedir('POST', `/nomina/${id}/aprobar`),
  });
})();
