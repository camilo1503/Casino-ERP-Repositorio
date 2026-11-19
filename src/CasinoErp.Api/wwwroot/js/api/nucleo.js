/* =====================================================================
   Núcleo de la conexión del prototipo PitBoss ERP (index.html) con la API.
   Dueño: Jorge · Contrato C2 (desarrollo/contratos.md).

   El prototipo guarda todo en memoria (objeto S). Cada módulo (js/api/*.js)
   registra sus "cargas" (qué pide a la API y cómo lo pasa a S) y envuelve
   las acciones del prototipo con conectar(): el prototipo actúa, la API
   confirma y luego se vuelve a sincronizar. Si la API rechaza la operación,
   se muestra su mensaje y la pantalla vuelve al estado real de la base.
   ===================================================================== */
(function () {
  const BASE = location.protocol === 'file:' ? 'http://localhost:5080' : '';
  const estado = { online: false };
  const cargas = [];

  const api = {
    estado,
    /** Encabezados extra en cada petición (seguridad.js agrega Authorization). */
    encabezados: () => ({}),
    /** Se llama con la respuesta cuando la API responde 401 (salvo en /auth/login). */
    alNoAutorizado: () => {},
    /** Si devuelve false, iniciar() no sincroniza (seguridad.js exige una sesión). */
    antesDeSincronizar: async () => true,
  };

  async function pedir(metodo, ruta, cuerpo) {
    const headers = Object.assign({}, cuerpo ? { 'Content-Type': 'application/json' } : {}, api.encabezados());
    const r = await fetch(BASE + '/api' + ruta, {
      method: metodo,
      headers,
      body: cuerpo ? JSON.stringify(cuerpo) : undefined,
    });
    const texto = await r.text();
    let datos = null;
    try { datos = texto ? JSON.parse(texto) : null; } catch { datos = null; }
    if (!r.ok) {
      if (r.status === 401 && ruta !== '/auth/login') api.alNoAutorizado(r);
      const mensaje = datos && (datos.mensaje || datos.title);
      if (r.status === 403 && !mensaje) throw new Error('No tienes acceso a este módulo.');
      throw new Error(mensaje || `Error ${r.status} de la API`);
    }
    return datos;
  }

  /**
   * Igual que pedir(), pero si hay sesión (S.user) y el rol no tiene ninguno de los módulos,
   * devuelve null sin llamar a la API. modulos: un nombre o una lista (basta uno).
   */
  function pedirSi(modulos, metodo, ruta, cuerpo) {
    const lista = Array.isArray(modulos) ? modulos : [modulos];
    if (S.user && !lista.some(allowed)) return Promise.resolve(null);
    return pedir(metodo, ruta, cuerpo);
  }

  /**
   * Registra una carga. obtener: async () => datos (o null para omitirla);
   * aplicar: datos => void (modifica S). Las cargas se aplican en orden ascendente.
   * Registrar otra vez el mismo nombre reemplaza la carga anterior.
   */
  function registrarCarga(nombre, orden, obtener, aplicar) {
    const i = cargas.findIndex(c => c.nombre === nombre);
    const carga = { nombre, orden, obtener, aplicar };
    if (i >= 0) cargas[i] = carga; else cargas.push(carga);
  }

  async function sincronizar() {
    const lista = [...cargas].sort((a, b) => a.orden - b.orden);
    const datos = await Promise.all(lista.map(c => c.obtener()));
    lista.forEach((c, i) => { if (datos[i] !== null && datos[i] !== undefined) c.aplicar(datos[i]); });
    render();
  }

  /**
   * tabla: ACT o FORM del prototipo. antes(...args) captura los datos antes de la acción
   * (null = no enviar), exito(ctx) dice si la acción local se aplicó y guardar(ctx) llama a la API.
   */
  function conectar(tabla, nombre, { antes = () => ({}), exito = () => true, guardar }) {
    const original = tabla[nombre];
    tabla[nombre] = function (...args) {
      const ctx = antes(...args);
      const resultado = original.apply(this, args);
      if (estado.online && ctx && exito(ctx, ...args)) {
        guardar(ctx, ...args)
          .then(sincronizar)
          .catch(err => { toast(err.message, true); sincronizar().catch(() => {}); });
      }
      return resultado;
    };
  }

  /** Prueba de vida, luego (si antesDeSincronizar lo permite) la primera sincronización. */
  async function iniciar() {
    await pedir('GET', '/salud');
    estado.online = true;
    try {
      if (await api.antesDeSincronizar()) await sincronizar();
    } catch (e) {
      estado.online = false;
      throw e;
    }
  }

  /* ---------- Utilidades: la API usa la fecha real; el prototipo, su fecha fija TODAY ---------- */
  const dos = n => String(n).padStart(2, '0');
  const isoLocal = d => `${d.getFullYear()}-${dos(d.getMonth() + 1)}-${dos(d.getDate())}`;
  const fechaApi = s => new Date(/Z$|[+-]\d\d:\d\d$/.test(s) ? s : s + 'Z');
  const hora = d => `${dos(d.getHours())}:${dos(d.getMinutes())}`;
  const sumarDias = (iso, n) => { const d = new Date(iso + 'T12:00:00'); d.setDate(d.getDate() + n); return isoLocal(d); };
  const hoyReal = isoLocal(new Date());
  const corrimiento = Math.round((new Date(TODAY + 'T12:00:00') - new Date(hoyReal + 'T12:00:00')) / 864e5);
  const aFront = iso => iso ? sumarDias(iso.slice(0, 10), corrimiento) : iso;
  const docApi = d => String(d || '').replace(/[.\s]/g, '').toUpperCase();
  const docFront = (tipo, doc) => tipo === 'Pasaporte' ? doc : grp(+doc);
  const codigoFicha = v => 'FICHA-' + (v >= 1e6 ? v / 1e6 + 'M' : v / 1000 + 'K');
  const MESES = ['enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio', 'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre'];

  Object.assign(api, {
    pedir, pedirSi, registrarCarga, sincronizar, conectar, iniciar,
    util: { dos, isoLocal, fechaApi, hora, sumarDias, hoyReal, corrimiento, aFront, docApi, docFront, codigoFicha, MESES },
  });
  window.PitBossApi = api;
})();
