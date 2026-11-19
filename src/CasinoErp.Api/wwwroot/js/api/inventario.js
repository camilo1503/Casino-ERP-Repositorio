/* =====================================================================
   Inventario de fichas de la bóveda. Dueño: Camilo · Contrato C2.
   Ver desarrollo/planes/Plan-Camilo.md (C6): aquí se agregan las alertas
   de stock crítico (carga 61) y el cambio de "Pedir al proveedor".
   ===================================================================== */
(function () {
  const { pedir, pedirSi, conectar, registrarCarga } = PitBossApi;
  const { codigoFicha } = PitBossApi.util;

  const MOD_INVENTARIO = ['inventario', 'caja'];

  function sincFichas(lista) {
    lista.forEach(f => { if (f.denominacion in S.vault) S.vault[f.denominacion] = f.stock; });
  }
  registrarCarga('fichas', 60, () => pedirSi(MOD_INVENTARIO, 'GET', '/inventario/fichas'), sincFichas);

  conectar(ACT, 'vaultOrder', {
    guardar: (ctx, v) => pedir('POST', `/inventario/fichas/${codigoFicha(+v)}/entradas`, { cantidad: 1500 }),
  });
  conectar(ACT, 'restock', {
    antes: id => { const t = table(id), n = Math.ceil(t.minChips * 1.4) - t.chips; return n > 0 ? { n } : null; },
    guardar: ({ n }) => pedir('POST', `/inventario/fichas/${codigoFicha(25000)}/retiros`, { cantidad: n }),
  });
})();
