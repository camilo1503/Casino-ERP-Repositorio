/* =====================================================================
   Módulos del Sprint 1 conectados a la API: Perfiles, Catálogo, Operación
   de mesas, Rellenos, Caja y Retención (y el estado de cumplimiento de las
   transacciones). Dueño: Jorge · Contrato C2. El inventario de fichas está
   en inventario.js (Camilo).
   ===================================================================== */
(function () {
  const { pedir, pedirSi, conectar, registrarCarga } = PitBossApi;
  const { fechaApi, hora, isoLocal, aFront, docApi, docFront, codigoFicha, hoyReal, MESES } = PitBossApi.util;

  /* ---------- Equivalencias entre el prototipo y la API ---------- */
  const EST_JUG = { ACTIVO: 'Activo', INACTIVO: 'Inactivo', AUTOEXCLUIDO: 'Autoexcluido' };
  const EST_TX = { APROBADA: 'Aprobada', EN_REVISION: 'En revisión', ANULADA: 'Anulada' };
  const TIPO_OF = { DESCUENTO_FICHAS: 'descuento', FICHAS_BONO: 'bono', BONO_RECLAMABLE: 'voucher' };
  const TIPO_API = { descuento: 'DESCUENTO_FICHAS', bono: 'FICHAS_BONO', voucher: 'BONO_RECLAMABLE' };
  const docDe = p => p && !p.soloLocal ? (p.apiDoc || docApi(p.doc)) : null;

  // Módulos que dan acceso a cada recurso (los mismos de [AutorizarModulo] en la API, contrato C5).
  const MOD_JUGADORES = ['perfiles', 'retencion', 'caja'];
  const MOD_MESAS = ['catalogo', 'mesas', 'mimesa'];
  const MOD_RELLENOS = ['mesas', 'inventario'];
  const MOD_CAJA = ['caja', 'cumplimiento'];
  const MOD_RETENCION = ['retencion', 'caja', 'mimesa'];

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

  function sincConfiguracion(c) {
    Object.assign(S.ret, {
      disc: c.porcentajeDescuento, cap: c.topeDescuento, bonus: c.fichasBono, voucher: c.bonoReclamable,
      days: c.diasVigencia, minMin: c.minutosMinimos, perDay: c.ofertasPorDia,
    });
    Object.assign(S.ret.on, { colorup: c.senalCambioFichas, salida: c.senalQuiereRetirarse, cobro: c.senalCobroCaja, app: c.senalAppJugador });
  }

  // En la primera carga, las señales bloqueadas del prototipo van al final del registro (como en el Sprint 1).
  let ofertasCargadas = false;
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
    bloqueadas.forEach(e => { if (e._orden === undefined) e._orden = ofertasCargadas ? ultimo + 0.5 : 0; });
    const deHoy = lista.filter(o => o.fecha === hoyReal).map(o => {
      const p = S.players.find(x => x.apiDoc === o.documento), k = o.tipo ? TIPO_OF[o.tipo] : null;
      return {
        t: hora(fechaApi(o.registradaEn)), pid: p && p.id, sig: o.senal, origin: o.origen,
        offer: k ? offerName(k) : 'Ofrecidas las 3 opciones', res: o.resultado === 'ACEPTADA' ? 'Aceptada' : 'Rechazada',
        val: k && k !== 'descuento' ? o.valor : 0, _orden: o.id,
      };
    });
    S.retLog = [...deHoy, ...bloqueadas].sort((a, b) => b._orden - a._orden);
    ofertasCargadas = true;
  }

  /* Cargas (orden del contrato C2) */
  registrarCarga('jugadores', 10, () => pedirSi(MOD_JUGADORES, 'GET', '/jugadores'), sincJugadores);
  registrarCarga('mesas', 20, async () => {
    const [mesas, rellenos] = await Promise.all([pedirSi(MOD_MESAS, 'GET', '/mesas'), pedirSi(MOD_RELLENOS, 'GET', '/rellenos')]);
    return mesas ? { mesas, rellenos: rellenos || [] } : null;
  }, d => sincMesas(d.mesas, d.rellenos));
  registrarCarga('transacciones', 30, () => pedirSi(MOD_CAJA, 'GET', '/caja/transacciones'), sincTransacciones);
  registrarCarga('configuracionRetencion', 40, () => pedirSi(MOD_RETENCION, 'GET', '/retencion/configuracion'), sincConfiguracion);
  registrarCarga('ofertas', 50, () => pedirSi(MOD_RETENCION, 'GET', '/retencion/ofertas'), sincOfertas);

  /* ---------- Envoltura de acciones: el prototipo actúa y la API confirma ---------- */
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

  /* Caja / POS (las fichas de la bóveda se mueven aquí; el resto del inventario está en inventario.js) */
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
})();
