/* =====================================================================
   Dueño: Camilo · Contrato C2 (desarrollo/contratos.md).
   Ver desarrollo/planes/Plan-Camilo.md (C6): módulo Proveedores y compras (carga 90).
   Usa window.PitBossApi y los globales del mockup; no edites index.html.
   ===================================================================== */
(function () {
  const { pedir, registrarCarga, sincronizar } = PitBossApi;
  const { codigoFicha } = PitBossApi.util;

  /* ---------- 1. Módulo en el menú (grupo Caja y bóveda) ---------- */
  MODS.proveedores = { l: 'Proveedores y compras', g: 3, tag: 'HU-05', sub: 'Proveedores de fichas y órdenes de compra automáticas' };
  ORDER.splice(ORDER.indexOf('inventario') + 1, 0, 'proveedores');
  ['gerente', 'jefepiso'].forEach(r => { if (ROLES[r] && !ROLES[r].views.includes('proveedores')) ROLES[r].views.push('proveedores'); });

  /* ---------- 2. Cargas (orden 90) ---------- */
  registrarCarga('proveedores', 90, () => allowed('proveedores') ? pedir('GET', '/proveedores') : null, l => { S.proveedores = l; });
  registrarCarga('ordenesCompra', 90, () => ['proveedores', 'inventario'].some(allowed) ? pedir('GET', '/compras/ordenes') : null, l => { S.ordenes = l; });

  const ESTADO = { BORRADOR: ['Borrador', 'warn'], ENVIADA: ['Enviada', 'gold'], RECIBIDA: ['Recibida', 'ok'], CANCELADA: ['Cancelada', ''] };
  const nombreProveedor = nit => { const p = (S.proveedores || []).find(x => x.nit === nit); return p ? p.nombre : (nit || 'Sin asignar'); };
  const activos = () => (S.proveedores || []).filter(p => p.activo);
  const err = m => m.err ? `<p class="err-msg">${esc(m.err)}</p>` : '';
  const pie = (texto, k = 'primary') => `<div class="mfoot"><button type="button" class="btn sec" data-act="closeModal">Cancelar</button><button class="btn ${k}" type="submit">${texto}</button></div>`;

  /* ---------- 3. Vista ---------- */
  V.proveedores = () => {
    if (!PitBossApi.estado.online || !S.ordenes) return card('Proveedores y compras', '<p class="hint">Este módulo necesita conexión con la API.</p>');
    const ps = S.proveedores || [], os = S.ordenes, mes = new Date().toISOString().slice(0, 7);
    const n = e => os.filter(o => o.estado === e).length;
    const nb = n('BORRADOR'), ne = n('ENVIADA'), nr = os.filter(o => o.estado === 'RECIBIDA' && (o.actualizadaEn || '').slice(0, 7) === mes).length;
    const admin = allowed('proveedores');
    const acciones = o => o.estado === 'BORRADOR'
      ? `<button class="btn dark sm" data-act="modal" data-arg="orderSend" data-id="${o.id}">Enviar</button> <button class="btn ghost sm" data-act="orderCancel" data-arg="${o.id}">Cancelar</button>`
      : o.estado === 'ENVIADA'
        ? `<button class="btn dark sm" data-act="orderReceive" data-arg="${o.id}">Recibir</button> <button class="btn ghost sm" data-act="orderCancel" data-arg="${o.id}">Cancelar</button>` : '';
    const ordenes = card('Órdenes de compra', `<div class="tblwrap"><table class="tbl"><thead><tr><th>Código</th><th>Ficha</th><th class="r">Cantidad</th><th>Proveedor</th><th>Origen</th><th>Estado</th><th>Acciones</th></tr></thead><tbody>${os.map(o => { const [l, k] = ESTADO[o.estado] || [o.estado, '']; return `<tr><td class="b">${o.codigo}</td><td>${esc(o.codigoFicha)}</td><td class="r b">${grp(o.cantidad)}</td><td>${esc(nombreProveedor(o.proveedorNit))}</td><td class="m">${esc(o.origen)}</td><td>${B(l, k)}</td><td style="white-space:nowrap">${acciones(o)}</td></tr>`; }).join('') || '<tr><td colspan="7"><div class="empty">No hay órdenes de compra.</div></td></tr>'}</tbody></table></div>`,
      `<button class="btn dark sm" data-act="modal" data-arg="orderNew">+ Orden manual</button>`);
    const proveedores = admin ? card('Proveedores', `<div class="tblwrap"><table class="tbl"><thead><tr><th>NIT</th><th>Nombre</th><th>Correo</th><th>Teléfono</th><th>Estado</th><th>Acciones</th></tr></thead><tbody>${ps.map(p => `<tr><td class="b">${esc(p.nit)}</td><td>${esc(p.nombre)}</td><td class="m">${esc(p.correo)}</td><td class="m">${esc(p.telefono)}</td><td>${B(p.activo ? 'Activo' : 'Inactivo', p.activo ? 'ok' : '')}</td><td style="white-space:nowrap"><button class="btn sec sm" data-act="modal" data-arg="supplier" data-id="${esc(p.nit)}">Editar</button> ${p.activo ? `<button class="btn ghost sm" data-act="supplierOff" data-arg="${esc(p.nit)}">Desactivar</button>` : `<button class="btn sec sm" data-act="supplierOn" data-arg="${esc(p.nit)}">Reactivar</button>`}</td></tr>`).join('')}</tbody></table></div>`,
      `<button class="btn dark sm" data-act="modal" data-arg="supplier">+ Registrar proveedor</button>`) : '';
    return kpis([['Proveedores activos', activos().length, 'de ' + ps.length], ['Órdenes en borrador', nb, 'Por enviar', nb ? 'warn' : 'ok'], ['Órdenes enviadas', ne, 'Por recibir'], ['Recibidas este mes', nr, 'Bóveda repuesta', 'ok']])
      + ordenes + proveedores;
  };

  /* ---------- 4. Modales ---------- */
  MODALS.supplier = m => {
    const p = m.id ? (S.proveedores || []).find(x => x.nit === m.id) : null, v = { ...(p || {}), ...(m.v || {}) };
    return `<h2>${p ? 'Editar proveedor' : 'Registrar proveedor'}</h2><p class="sub">HU-05 · NIT de 9 dígitos y dígito de verificación (901234567-1). Desactivar no borra el proveedor.</p><form class="form" data-form="supplier"><input type="hidden" name="edit" value="${p ? esc(p.nit) : ''}"><div class="row"><label class="field"><span>NIT</span><input class="in" name="nit" id="pv-nit" value="${esc(v.nit)}" placeholder="901234567-1" ${p ? 'readonly' : ''}></label><label class="field"><span>Nombre</span><input class="in" name="name" id="pv-name" value="${esc(v.nombre)}"></label></div><div class="row"><label class="field"><span>Correo</span><input class="in" name="email" id="pv-email" inputmode="email" value="${esc(v.correo)}"></label><label class="field"><span>Teléfono</span><input class="in" name="phone" id="pv-phone" value="${esc(v.telefono)}"></label></div>${err(m)}${pie(p ? 'Guardar' : 'Registrar')}</form>`;
  };
  MODALS.orderSend = m => {
    const o = (S.ordenes || []).find(x => String(x.id) === String(m.id));
    return `<h2>Enviar orden ${o ? o.codigo : ''}</h2><p class="sub">${o ? esc(o.codigoFicha) + ' · ' + grp(o.cantidad) + ' fichas' : ''}</p><form class="form" data-form="orderSend"><input type="hidden" name="id" value="${esc(m.id)}"><label class="field"><span>Proveedor</span><select class="in" name="nit" id="os-nit">${activos().map(p => `<option value="${esc(p.nit)}" ${o && o.proveedorNit === p.nit ? 'selected' : ''}>${esc(p.nombre)} · ${esc(p.nit)}</option>`).join('')}</select></label>${err(m)}${pie('Enviar orden')}</form>`;
  };
  MODALS.orderNew = m => `<h2>Orden de compra manual</h2><p class="sub">Queda en borrador hasta que se envía al proveedor.</p><form class="form" data-form="orderNew"><div class="row"><label class="field"><span>Denominación</span><select class="in" name="den" id="on-den">${DENOMS.map(([v]) => `<option value="${v}">${money(v)}</option>`).join('')}</select></label><label class="field"><span>Cantidad</span><input class="in num" type="number" name="qty" id="on-qty" value="1500" min="1"></label></div><label class="field"><span>Proveedor (opcional)</span><select class="in" name="nit" id="on-nit"><option value="">Asignar después</option>${activos().map(p => `<option value="${esc(p.nit)}">${esc(p.nombre)}</option>`).join('')}</select></label>${err(m)}${pie('Crear orden')}</form>`;

  /* ---------- 5. Acciones ---------- */
  async function hacer(promesa, mensaje) {
    try {
      await promesa;
      S.modal = null;
      toast(mensaje);
      await sincronizar();
    } catch (e) {
      if (S.modal) { S.modal.err = e.message; render(); } else toast(e.message, true);
    }
  }

  FORM.supplier = fd => {
    const edit = fd.get('edit'), body = { nit: (fd.get('nit') || '').trim(), nombre: fd.get('name'), correo: fd.get('email'), telefono: fd.get('phone') };
    S.modal.v = { nit: body.nit, nombre: body.nombre, correo: body.correo, telefono: body.telefono };
    hacer(edit ? pedir('PUT', `/proveedores/${encodeURIComponent(edit)}`, body) : pedir('POST', '/proveedores', body),
      edit ? 'Proveedor actualizado.' : 'Proveedor registrado.');
  };
  FORM.orderSend = fd => hacer(pedir('POST', `/compras/ordenes/${fd.get('id')}/enviar`, { nit: fd.get('nit') || null }), 'Orden enviada.');
  FORM.orderNew = fd => hacer(pedir('POST', '/compras/ordenes', { codigoFicha: codigoFicha(+fd.get('den')), cantidad: +fd.get('qty'), nit: fd.get('nit') || null }), 'Orden de compra creada.');
  ACT.supplierOff = nit => { hacer(pedir('POST', `/proveedores/${encodeURIComponent(nit)}/desactivar`), 'Proveedor desactivado.'); return false; };
  ACT.supplierOn = nit => { hacer(pedir('POST', `/proveedores/${encodeURIComponent(nit)}/reactivar`), 'Proveedor reactivado.'); return false; };
  ACT.orderReceive = id => { hacer(pedir('POST', `/compras/ordenes/${id}/recibir`), 'Orden recibida: el stock de la bóveda se actualizó.'); return false; };
  ACT.orderCancel = id => { hacer(pedir('POST', `/compras/ordenes/${id}/cancelar`), 'Orden cancelada.'); return false; };
})();
