/* =====================================================================
   Arranque de la conexión con la API (se carga al final). Dueño: Jorge.
   Si la API no responde, el prototipo sigue en modo demostración.
   ===================================================================== */
PitBossApi.iniciar()
  .catch(() => toast('Sin conexión con la API: el ERP funciona en modo demostración y los cambios no se guardan.', true));
