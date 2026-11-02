/* =====================================================================
   Conexión del prototipo PitBoss ERP (index.html) con la API del ERP.

   El prototipo guarda todo en memoria (objeto S). Este archivo no cambia
   ninguna pantalla: envuelve las acciones de los módulos del incremento
   para que también se guarden en la API (ASP.NET Core + SQL Server) y,
   después de cada operación, vuelve a cargar los datos desde la API.
   Si la API rechaza una operación, se muestra su mensaje y se recargan
   los datos (la pantalla vuelve al estado real de la base de datos).

   Módulos conectados: Perfiles, Catálogo, Operación de mesas, Caja,
   Inventario de fichas, Retención y el estado de cumplimiento de las
   transacciones. Los demás siguen con los datos de ejemplo del prototipo.
   ===================================================================== */
(function () {
  const BASE = location.protocol === 'file:' ? 'http://localhost:5080' : '';
  const API = { online: false };

  async function pedir(metodo, ruta, cuerpo) {
    const r = await fetch(BASE + '/api' + ruta, {
      method: metodo,
      headers: cuerpo ? { 'Content-Type': 'application/json' } : {},
      body: cuerpo ? JSON.stringify(cuerpo) : undefined,
    });
    const texto = await r.text();
    const datos = texto ? JSON.parse(texto) : null;
    if (!r.ok) throw new Error((datos && (datos.mensaje || datos.title)) || `Error ${r.status} de la API`);
    return datos;
  }

  /* ---------- Fechas: la API usa la fecha real; el prototipo, su fecha fija TODAY ---------- */
  const dos = n => String(n).padStart(2, '0');
  const isoLocal = d => `${d.getFullYear()}-${dos(d.getMonth() + 1)}-${dos(d.getDate())}`;
  const fechaApi = s => new Date(/Z$|[+-]\d\d:\d\d$/.test(s) ? s : s + 'Z');
  const hora = d => `${dos(d.getHours())}:${dos(d.getMinutes())}`;
  const sumarDias = (iso, n) => { const d = new Date(iso + 'T12:00:00'); d.setDate(d.getDate() + n); return isoLocal(d); };
  const hoyReal = isoLocal(new Date());
  const corrimiento = Math.round((new Date(TODAY + 'T12:00:00') - new Date(hoyReal + 'T12:00:00')) / 864e5);
  const aFront = iso => iso ? sumarDias(iso.slice(0, 10), corrimiento) : iso;

  /* ---------- Equivalencias entre el prototipo y la API ---------- */
  const docApi = d => String(d || '').replace(/[.\s]/g, '').toUpperCase();
  const docFront = (tipo, doc) => tipo === 'Pasaporte' ? doc : grp(+doc);
  const EST_JUG = { ACTIVO: 'Activo', INACTIVO: 'Inactivo', AUTOEXCLUIDO: 'Autoexcluido' };
  const EST_TX = { APROBADA: 'Aprobada', EN_REVISION: 'En revisión', ANULADA: 'Anulada' };
  const TIPO_OF = { DESCUENTO_FICHAS: 'descuento', FICHAS_BONO: 'bono', BONO_RECLAMABLE: 'voucher' };
  const TIPO_API = { descuento: 'DESCUENTO_FICHAS', bono: 'FICHAS_BONO', voucher: 'BONO_RECLAMABLE' };
  const MESES = ['enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio', 'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre'];
  const codigoFicha = v => 'FICHA-' + (v >= 1e6 ? v / 1e6 + 'M' : v / 1000 + 'K');
  const docDe = p => p && !p.soloLocal ? (p.apiDoc || docApi(p.doc)) : null;

  /* ---------- Carga desde la API ---------- */
  function sincJugadores(lista) {
    const previos = S.players;
    const desdeApi = lista.map(j => {
      let p = previos.find(x => !x.soloLocal && docApi(x.doc) === j.documento);
      if (!p) {
        const r = fechaApi(j.fechaRegistro);
        p = { id: 'pa' + j.id, since: `${MESES[r.getMonth()]} ${r.getFullYear()}`, last: 'Hoy ' + hora(new Date()), visits: 1, hist: [], benefits: [], red: [] };
      }
      return Object.assign(p, {
        apiDoc: j.documento, name: j.nombre, docT: j.tipoDocumento, doc: docFront(j.tipoDocumento, j.documento),
        email: j.correo, phone: j.telefono || '', city: j.ciudad, birth: j.fechaNacimiento, points: j.puntos,
        prefs: j.preferencias, slot: j.franja, avg: j.apuestaPromedio, st: EST_JUG[j.estado], reason: j.motivoEstado || '',
      });
    });
    // Los registrados desde la app del jugador no tienen documento todavía: se quedan solo en el prototipo.
    S.players = [...desdeApi, ...previos.filter(p => p.soloLocal)];
  }

  function sincMesas(mesas, rellenos) {
    const pendientes = rellenos.filter(r => r.estado === 'PENDIENTE');
    const previas = S.tables;
    S.tables = mesas.map(m => {
      const t = previas.find(x => x.id === m.numero) || {
        id: m.numero, dealer: null, drop: 0, chips: 0, minChips: m.juego === 'Tragamonedas' ? 0 : 600,
        opened: null, openAmt: 15000000, fills: 0, count: '—', seated: [], free: m.asientos,
      };
      const abierta = m.estado === 'ABIERTA';
      const st = abierta ? (pendientes.some(r => r.mesaNumero === m.numero) ? 'r' : 'a')
        : { CERRADA: 'c', MANTENIMIENTO: 'm', RETIRADA: 'x' }[m.estado];
      Object.assign(t, { game: m.juego, zone: m.zona, seats: m.asientos, min: m.apuestaMinima, max: m.apuestaMaxima, st });
      if (m.motivoRetiro) t.retired = m.motivoRetiro;
      if (!abierta) {
        const d = staff(t.dealer);
        if (d) d.area = 'Sin asignar';
        Object.assign(t, { dealer: null, seated: [], free: t.seats, opened: null });
      } else if (!t.opened) t.opened = '14:00';
      return t;
    });
    S.fills = pendientes.map(r => ({ table: r.mesaNumero, amount: r.monto, by: r.solicitadoPor, time: hora(fechaApi(r.solicitadoEn)), apiId: r.id }));
  }

  function sincTransacciones(lista) {
    S.tx = lista.map(t => {
      const f = fechaApi(t.fecha), p = t.documento ? S.players.find(x => x.apiDoc === t.documento) : null;
      const tx = {
        id: t.codigo, apiId: t.id, pid: p ? p.id : null, anon: p ? null : 'Jugador no registrado', doc: p ? null : 'Sin documento',
        type: t.tipo === 'COMPRA' ? 'Compra' : 'Pago', amount: t.monto, disc: t.descuento || 0,
        date: aFront(isoLocal(f)), time: hora(f), status: EST_TX[t.estado], where: t.ubicacion || t.caja,
      };
      if (t.caja !== 'Caja 2') tx.caja = t.caja;
      if (t.estadoCumplimiento) tx.cstatus = t.estadoCumplimiento;
      return tx;
    });
  }

  function sincFichas(lista) {
    lista.forEach(f => { if (f.denominacion in S.vault) S.vault[f.denominacion] = f.stock; });
  }

  function sincConfiguracion(c) {
    Object.assign(S.ret, {
      disc: c.porcentajeDescuento, cap: c.topeDescuento, bonus: c.fichasBono, voucher: c.bonoReclamable,
      days: c.diasVigencia, minMin: c.minutosMinimos, perDay: c.ofertasPorDia,
    });
    Object.assign(S.ret.on, { colorup: c.senalCambioFichas, salida: c.senalQuiereRetirarse, cobro: c.senalCobroCaja, app: c.senalAppJugador });
  }

  function sincOfertas(lista) {
    const estadoPrevio = {};
    S.players.forEach(p => p.benefits.forEach(b => { estadoPrevio[b.id] = b.st; }));
    S.players.forEach(p => { if (!p.soloLocal) p.benefits = []; });
    lista.filter(o => o.resultado === 'ACEPTADA').forEach(o => {
      const p = S.players.find(x => x.apiDoc === o.documento), k = TIPO_OF[o.tipo];
      if (!p) return;
      p.benefits.push({
        id: o.codigo, type: k, value: o.valor, cap: S.ret.cap, exp: aFront(o.vence),
        st: estadoPrevio[o.codigo] || (k === 'descuento' ? 'Activo' : k === 'bono' ? 'Entregado' : 'Emitido'),
      });
    });
    // El registro del día: lo guardado en la API más las señales bloqueadas (no generan oferta).
    // Orden: lo más reciente arriba. Una señal bloqueada queda justo encima de la última oferta que existía cuando ocurrió.
    const ultimo = lista.reduce((m, o) => Math.max(m, o.id), 0);
    const bloqueadas = S.retLog.filter(e => e.res === 'Bloqueada');
    bloqueadas.forEach(e => { if (e._orden === undefined) e._orden = API.online ? ultimo + 0.5 : 0; });
    const deHoy = lista.filter(o => o.fecha === hoyReal).map(o => {
      const p = S.players.find(x => x.apiDoc === o.documento), k = o.tipo ? TIPO_OF[o.tipo] : null;
      return {
        t: hora(fechaApi(o.registradaEn)), pid: p && p.id, sig: o.senal, origin: o.origen,
        offer: k ? offerName(k) : 'Ofrecidas las 3 opciones', res: o.resultado === 'ACEPTADA' ? 'Aceptada' : 'Rechazada',
        val: k && k !== 'descuento' ? o.valor : 0, _orden: o.id,
      };
    });
    S.retLog = [...deHoy, ...bloqueadas].sort((a, b) => b._orden - a._orden);
  }

  async function sincronizar() {
    const [jugadores, mesas, rellenos, transacciones, fichas, ofertas, configuracion] = await Promise.all([
      pedir('GET', '/jugadores'), pedir('GET', '/mesas'), pedir('GET', '/rellenos'), pedir('GET', '/caja/transacciones'),
      pedir('GET', '/inventario/fichas'), pedir('GET', '/retencion/ofertas'), pedir('GET', '/retencion/configuracion'),
    ]);
    sincJugadores(jugadores);
    sincMesas(mesas, rellenos);
    sincTransacciones(transacciones);
    sincFichas(fichas);
    sincConfiguracion(configuracion);
    sincOfertas(ofertas);
    render();
  }

  /* ---------- Envoltura de acciones: el prototipo actúa y la API confirma ---------- */
  /**
   * tabla: ACT o FORM del prototipo. antes(...args) captura los datos antes de la acción
   * (null = no enviar), exito(ctx) dice si la acción local se aplicó y guardar(ctx) llama a la API.
   */
  function conectar(tabla, nombre, { antes = () => ({}), exito = () => true, guardar }) {
    const original = tabla[nombre];
    tabla[nombre] = function (...args) {
      const ctx = antes(...args);
      const resultado = original.apply(this, args);
      if (API.online && ctx && exito(ctx, ...args)) {
        guardar(ctx, ...args)
          .then(sincronizar)
          .catch(err => { toast(err.message, true); sincronizar().catch(() => {}); });
      }
      return resultado;
    };
  }
  const modalCerrado = () => S.modal === null;

  /* Perfiles de jugador (E-01) */
  conectar(FORM, 'register', {
    antes: fd => ({ n: S.players.length, docT: fd.get('docT'), doc: docApi(fd.get('doc')) }),
    exito: ctx => S.players.length > ctx.n,
    guardar: (ctx, fd) => pedir('POST', '/jugadores', {
      tipoDocumento: ctx.docT, documento: ctx.doc, nombre: fd.get('name').trim(),
      correo: fd.get('email').trim(), fechaNacimiento: fd.get('birth'),
    }),
  });
  conectar(FORM, 'editProfile', {
    antes: () => ({ p: player(S.ui.sel) }),
    exito: modalCerrado,
    guardar: ({ p }) => pedir('PUT', `/jugadores/${docDe(p)}/perfil`, {
      telefono: p.phone, ciudad: p.city, franja: p.slot, apuestaPromedio: p.avg, preferencias: p.prefs,
    }),
  });
  conectar(FORM, 'deactivate', {
    antes: () => ({ p: player(S.ui.sel) }),
    exito: modalCerrado,
    guardar: ({ p }, fd) => pedir('POST', `/jugadores/${docDe(p)}/${fd.get('st') === 'Autoexcluido' ? 'autoexcluir' : 'desactivar'}`,
      { motivo: fd.get('reason').trim() }),
  });
  conectar(ACT, 'reactivate', {
    antes: () => ({ p: player(S.ui.sel) }),
    guardar: ({ p }) => pedir('POST', `/jugadores/${docDe(p)}/reactivar`),
  });
  conectar(FORM, 'pReg', {
    antes: () => ({ n: S.players.length }),
    exito: ctx => { const p = player(S.pa.pid); if (S.players.length > ctx.n && p) p.soloLocal = true; return false; },
    guardar: async () => {},
  });

  /* Catálogo y operación de mesas (E-02) */
  const rechazarRellenos = (numero, motivo) => Promise.all(S.fills.filter(f => f.table === numero && f.apiId)
    .map(f => pedir('POST', `/rellenos/${f.apiId}/rechazar`, { motivo, rechazadoPor: cur().name })));
  async function llevarMesaA(numero, desde, hasta) {
    const de = desde === 'r' ? 'a' : desde, a = hasta === 'r' ? 'a' : hasta;
    if (de === a) return;
    if (de === 'a') { await rechazarRellenos(numero, 'Mesa cerrada'); await pedir('POST', `/mesas/${numero}/cerrar`); }
    if (de === 'm') await pedir('POST', `/mesas/${numero}/reparada`);
    if (a === 'a') await pedir('POST', `/mesas/${numero}/abrir`);
    if (a === 'm') await pedir('POST', `/mesas/${numero}/mantenimiento`);
  }
  conectar(FORM, 'tableForm', {
    antes: fd => { const edit = fd.get('edit'), t = edit && table(edit); return { edit, desde: t ? t.st : null }; },
    exito: modalCerrado,
    guardar: async (ctx, fd) => {
      const min = +fd.get('min'), max = +fd.get('max'), zona = fd.get('zone');
      if (!ctx.edit) {
        return pedir('POST', '/mesas', { numero: fd.get('id').trim().toUpperCase(), juego: fd.get('game'), apuestaMinima: min, apuestaMaxima: max, zona });
      }
      await pedir('PUT', `/mesas/${ctx.edit}/limites`, { apuestaMinima: min, apuestaMaxima: max, zona });
      await llevarMesaA(ctx.edit, ctx.desde, table(ctx.edit).st);
    },
  });
  conectar(FORM, 'retireTable', {
    exito: modalCerrado,
    guardar: (ctx, fd) => pedir('POST', `/mesas/${fd.get('id')}/retirar`, { motivo: fd.get('reason') || 'Sin motivo' }),
  });
  conectar(FORM, 'dealer', {
    antes: fd => fd.get('open') ? { numero: S.ui.table } : null,
    exito: modalCerrado,
    guardar: ({ numero }) => pedir('POST', `/mesas/${numero}/abrir`),
  });
  conectar(ACT, 'doCloseTable', {
    antes: () => ({ numero: S.ui.table, desde: table(S.ui.table).st }),
    guardar: ({ numero, desde }) => llevarMesaA(numero, desde, 'c'),
  });
  conectar(ACT, 'tableMaint', { guardar: (ctx, id) => pedir('POST', `/mesas/${id}/mantenimiento`) });
  conectar(ACT, 'tableFixed', { guardar: (ctx, id) => pedir('POST', `/mesas/${id}/reparada`) });

  /* Rellenos de fichas y bóveda */
  conectar(FORM, 'fillReq', {
    exito: modalCerrado,
    guardar: (ctx, fd) => pedir('POST', '/rellenos', { mesaNumero: fd.get('id'), monto: +fd.get('a'), solicitadoPor: cur().name }),
  });
  conectar(ACT, 'approveFill', {
    antes: id => { const f = S.fills.find(x => x.table === id); return f && f.apiId ? { apiId: f.apiId } : null; },
    guardar: ({ apiId }) => pedir('POST', `/rellenos/${apiId}/aprobar`, { aprobadoPor: cur().name }),
  });

  /* Caja / POS e inventario de la bóveda */
  conectar(FORM, 'tx', {
    antes: () => {
      const d = S.txd, total = DENOMS.reduce((s, [v]) => s + v * (d.q[v] || 0), 0), p = player(d.pid);
      if (!total) return null;
      const disc = p && d.type === 'Compra' ? p.benefits.find(b => b.type === 'descuento' && b.st === 'Activo') : null;
      const descuento = disc ? Math.min(Math.round(total * disc.value / 100), disc.cap) : 0;
      return { tipo: d.type, documento: docDe(p), total, descuento, q: { ...d.q } };
    },
    guardar: async c => {
      await pedir('POST', '/caja/transacciones', {
        tipo: c.tipo === 'Compra' ? 'COMPRA' : 'PAGO', documento: c.documento, monto: c.total, descuento: c.descuento, caja: 'Caja 2', ubicacion: 'Caja 2',
      });
      // Compra: las fichas salen de la bóveda. Pago: el jugador entrega fichas y vuelven a la bóveda.
      for (const [v] of DENOMS) {
        const n = c.q[v] || 0;
        if (n) await pedir('POST', `/inventario/fichas/${codigoFicha(v)}/${c.tipo === 'Compra' ? 'retiros' : 'entradas'}`, { cantidad: n });
      }
    },
  });
  const porTransaccion = (ruta, cuerpo) => ({
    antes: id => { const t = S.tx.find(x => x.id === id); return t && t.apiId ? { apiId: t.apiId } : null; },
    guardar: ({ apiId }) => pedir('POST', `/caja/transacciones/${apiId}/${ruta}`, cuerpo),
  });
  conectar(ACT, 'txApprove', porTransaccion('aprobar'));
  conectar(ACT, 'txVoid', porTransaccion('anular'));
  conectar(ACT, 'txReport', porTransaccion('cumplimiento', { estado: 'Reportada' }));
  conectar(ACT, 'txEscalate', porTransaccion('cumplimiento', { estado: 'Escalada' }));
  conectar(ACT, 'vaultOrder', {
    guardar: (ctx, v) => pedir('POST', `/inventario/fichas/${codigoFicha(+v)}/entradas`, { cantidad: 1500 }),
  });
  conectar(ACT, 'restock', {
    antes: id => { const t = table(id), n = Math.ceil(t.minChips * 1.4) - t.chips; return n > 0 ? { n } : null; },
    guardar: ({ n }) => pedir('POST', `/inventario/fichas/${codigoFicha(25000)}/retiros`, { cantidad: n }),
  });

  /* Retención de jugadores (E-03) */
  const respuestaOferta = (aceptada, desdeApp) => ({
    antes: k => {
      const m = desdeApp
        ? { pid: S.pa.pid, sig: 'Botón "Me retiro" en la app', origin: 'App del jugador', min: S.pa.offerMin }
        : S.modal;
      const p = m && player(m.pid);
      return p && docDe(p) ? { documento: docDe(p), min: m.min || 0, sig: m.sig, origin: m.origin, tipo: aceptada ? TIPO_API[k] : null } : null;
    },
    guardar: c => pedir('POST', '/retencion/ofertas', {
      documento: c.documento, minutosSesion: c.min, senal: c.sig, origen: c.origin, aceptada, tipo: c.tipo,
    }),
  });
  conectar(ACT, 'retAccept', respuestaOferta(true, false));
  conectar(ACT, 'retReject', respuestaOferta(false, false));
  conectar(ACT, 'pTakeOffer', respuestaOferta(true, true));
  conectar(ACT, 'pLeave', respuestaOferta(false, true));
  conectar(FORM, 'retConfig', {
    exito: (ctx, fd) => ['disc', 'cap', 'bonus', 'voucher', 'minMin', 'perDay'].every(k => S.ret[k] === +fd.get(k)),
    guardar: () => pedir('PUT', '/retencion/configuracion', {
      porcentajeDescuento: S.ret.disc, topeDescuento: S.ret.cap, fichasBono: S.ret.bonus, bonoReclamable: S.ret.voucher,
      diasVigencia: S.ret.days, minutosMinimos: S.ret.minMin, ofertasPorDia: S.ret.perDay,
      senalCambioFichas: S.ret.on.colorup, senalQuiereRetirarse: S.ret.on.salida, senalCobroCaja: S.ret.on.cobro, senalAppJugador: S.ret.on.app,
    }),
  });

  /* ---------- Arranque ---------- */
  sincronizar()
    .then(() => { API.online = true; })
    .catch(() => toast('Sin conexión con la API: el ERP funciona en modo demostración y los cambios no se guardan.', true));
  window.PitBossApi = { sincronizar, estado: API };
})();
