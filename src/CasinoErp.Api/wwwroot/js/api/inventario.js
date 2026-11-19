/* =====================================================================
   Inventario de fichas de la bóveda. Dueño: Camilo · Contrato C2.
   Ver desarrollo/planes/Plan-Camilo.md (C6):
   - carga "fichas" (60): stock y umbral mínimo de cada denominación;
   - carga "alertasStock" (61): alertas abiertas y toast cuando aparece una nueva;
   - alerts() y V.inventario usan las alertas y los umbrales reales;
   - "Pedir al proveedor" crea una orden de compra en borrador (no suma stock).
   Sin API (modo demostración) todo sigue funcionando con los datos del mockup.
   ===================================================================== */
(function () {
  const { pedir, pedirSi, conectar, registrarCarga, sincronizar } = PitBossApi;
  const { codigoFicha } = PitBossApi.util;

  const MOD_INVENTARIO = ['inventario', 'caja'];
  const denominacionDe = codigo => {
    const m = /^FICHA-(\d+)([KM]?)$/.exec(codigo || '');
    return m ? +m[1] * (m[2] === 'K' ? 1000 : m[2] === 'M' ? 1000000 : 1) : 0;
  };
  const umbral = v => (S.umbralBoveda || {})[v] ?? 1000;

  /* ---------- 1. Carga "fichas" (60): stock y umbral por denominación ---------- */
  registrarCarga('fichas', 60, () => pedirSi(MOD_INVENTARIO, 'GET', '/inventario/fichas'), lista => {
    S.umbralBoveda = {};
    lista.forEach(f => {
      if (f.denominacion in S.vault) S.vault[f.denominacion] = f.stock;
      S.umbralBoveda[f.denominacion] = f.stockMinimo;
    });
  });

  /* ---------- 2. Carga "alertasStock" (61) ---------- */
  registrarCarga('alertasStock', 61,
    () => (!S.user || MOD_INVENTARIO.some(allowed)) ? pedir('GET', '/inventario/alertas') : null,
    lista => {
      if (S.alertasApi) {
        const antes = new Set(S.alertasApi.map(a => a.codigoFicha));
        lista.filter(a => !antes.has(a.codigoFicha)).forEach(a =>
          toast('Stock crítico: ' + a.codigoFicha + ' quedó en ' + grp(a.stockActual) + ' unidades (mínimo ' + grp(a.stockMinimo) + '). Se creó un borrador de orden de compra.', true));
      }
      S.alertasApi = lista;
    });

  /* ---------- 3. alerts(): suma las alertas de la bóveda ----------
     Van con las demás alertas "bad" (antes de las "warn"), porque el panel de inicio
     solo muestra las cuatro primeras. */
  const alertsLocal = alerts;
  alerts = function () {
    const a = alertsLocal();
    if (!allowed('inventario')) return a;
    const boveda = (S.alertasApi || []).map(x => ({
      k: 'bad', t: 'Stock de fichas crítico', go: 'inventario',
      d: 'Bóveda · ' + money(denominacionDe(x.codigoFicha)) + ' · ' + grp(x.stockActual) + ' und. (mínimo ' + grp(x.stockMinimo) + ')',
    }));
    const i = a.findIndex(x => x.k !== 'bad');
    a.splice(i < 0 ? a.length : i, 0, ...boveda);
    return a;
  };

  /* ---------- 4. V.inventario con alertas y umbrales reales ---------- */
  V.inventario = () => {
    const tabs = liveTables().filter(t => t.minChips); const st = t => t.chips < t.minChips ? ['Crítico', 'bad'] : t.chips < t.minChips * 1.15 ? ['Bajo', 'warn'] : ['Normal', 'ok'];
    const crit = tabs.filter(t => st(t)[0] === 'Crítico');
    const low = S.alertasApi ? DENOMS.filter(([v]) => S.alertasApi.some(a => a.codigoFicha === codigoFicha(v))) : DENOMS.filter(([v]) => S.vault[v] < 1000);
    const vaultVal = DENOMS.reduce((s, [v]) => s + v * S.vault[v], 0), circ = tabs.reduce((s, t) => s + t.chips, 0);
    const W = 640, H = 230, b = 190, mx = 5000, sc = (b - 20) / mx;
    const chart = `<svg viewBox="0 0 ${W} ${H}" width="100%" role="img" aria-label="Fichas en bóveda por denominación">${[0, 1000, 2000, 3000, 4000, 5000].map(v => `<line class="grid-l" x1="50" x2="${W}" y1="${b - v * sc}" y2="${b - v * sc}"/><text class="ax" x="44" y="${b - v * sc + 4}" text-anchor="end">${grp(v)}</text>`).join('')}${DENOMS.map(([v], i) => { const n = Math.min(S.vault[v], mx), h = Math.max(n * sc, 2), x = 80 + i * 95, u = Math.min(umbral(v), mx), bajo = low.some(([d]) => d === v); return `<rect x="${x}" y="${b - h}" width="56" height="${h}" rx="6" fill="${bajo ? 'var(--bad)' : 'var(--brand)'}"/><line x1="${x - 8}" x2="${x + 64}" y1="${b - u * sc}" y2="${b - u * sc}" stroke="var(--bad)" stroke-width="1.5" stroke-dasharray="6 4"/><text x="${x + 28}" y="${b - h - 8}" text-anchor="middle" font-size="12" font-weight="700" fill="var(--fg)">${grp(S.vault[v])}</text><text class="ax" x="${x + 28}" y="${b + 20}" text-anchor="middle">${money(v)}</text>`; }).join('')}</svg>`;
    return kpis([['Fichas en mesas', grp(circ), tabs.length + ' mesas'], ['Valor en bóveda', money(vaultVal), 'Último conteo 18:00'], ['Mesas activas', tabs.filter(t => t.st === 'a' || t.st === 'r').length + ' / ' + tabs.length, 'Zona A'], ['Alertas de stock', crit.length + low.length, 'Requieren reposición', crit.length + low.length ? 'bad' : 'ok']]) +
      `<div class="g2">${card('Stock de fichas por mesa', `<div class="tblwrap"><table class="tbl"><thead><tr><th>Mesa</th><th>Juego</th><th class="r">Fichas</th><th class="r">Mínimo</th><th>Último conteo</th><th>Estado</th><th></th></tr></thead><tbody>${tabs.map(t => { const [l, k] = st(t); return `<tr><td class="b">${t.id}</td><td>${t.game}</td><td class="r b">${grp(t.chips)}</td><td class="r">${grp(t.minChips)}</td><td class="m">Hoy ${t.count}</td><td>${B(l, k)}</td><td>${l !== 'Normal' ? `<button class="btn dark sm" data-act="restock" data-arg="${t.id}">Reponer</button>` : ''}</td></tr>`; }).join('')}</tbody></table></div>`, `<button class="btn dark sm" data-act="modal" data-arg="count">+ Registrar conteo</button>`)}
  ${card('Alertas de stock', crit.length + low.length ? `<div class="list">${crit.map(t => `<div class="alert"><b>Mesa ${t.id} · ${t.game}</b><p>${grp(t.chips)} fichas · mínimo ${grp(t.minChips)}</p><button class="btn dark sm" style="margin-top:8px" data-act="restock" data-arg="${t.id}">Reponer</button></div>`).join('')}${low.map(([v]) => `<div class="alert warn"><b>Bóveda · fichas de ${money(v)}</b><p>${grp(S.vault[v])} und. · umbral ${grp(umbral(v))}</p><button class="btn dark sm" style="margin-top:8px" data-act="vaultOrder" data-arg="${v}">Pedir al proveedor</button></div>`).join('')}</div>` : '<div class="empty">Todo el stock está sobre el mínimo.</div>')}</div>
  ${card('Nivel de fichas en bóveda por denominación', chart, '<span class="hint">La línea roja es el umbral mínimo de cada denominación</span>')}`;
  };

  /* ---------- 5. "Pedir al proveedor": orden de compra en borrador ---------- */
  const original = ACT.vaultOrder;
  ACT.vaultOrder = v => {
    if (!PitBossApi.estado.online) return original(v);
    pedir('POST', '/compras/ordenes', { codigoFicha: codigoFicha(+v), cantidad: 1500, nit: null })
      .then(() => { toast('Orden de compra creada en borrador. Revísala en Proveedores y compras.'); return sincronizar(); })
      .catch(e => toast(e.message, true));
    return false;
  };

  /* ---------- 6. Reponer una mesa desde la bóveda (Fase 0) ---------- */
  conectar(ACT, 'restock', {
    antes: id => { const t = table(id), n = Math.ceil(t.minChips * 1.4) - t.chips; return n > 0 ? { n } : null; },
    guardar: ({ n }) => pedir('POST', `/inventario/fichas/${codigoFicha(25000)}/retiros`, { cantidad: n }),
  });
})();
