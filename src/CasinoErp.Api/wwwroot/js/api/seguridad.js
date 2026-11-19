/* =====================================================================
   Dueño: Iván · Contrato C2 (desarrollo/contratos.md).
   Ver desarrollo/planes/Plan-Ivan.md (I6): sesión JWT, login con 2FA, usuarios y roles.
   Usa window.PitBossApi y los globales del mockup; no edites index.html.

   - Sesión: el token de POST /api/auth/login se guarda en sessionStorage y viaja en Authorization.
     Al recargar (F5) la sesión se restaura con GET /api/auth/yo. Un 401 cierra la sesión.
   - Login: con la API en línea se escribe el correo (datalist con las cuentas conocidas).
   - Usuarios y roles: crear, cambiar rol, restablecer, desactivar y reactivar van a /api/usuarios.
   - Cambio de contraseña obligatorio cuando la cuenta tiene contraseña temporal.
   Sin API (modo demostración) se usa el login local del mockup.
   ===================================================================== */
(function () {
  const { pedir, pedirSi, conectar, registrarCarga, util } = PitBossApi;
  const { fechaApi, hora, dos, docApi } = util;

  /* Claves de rol: API (contrato C7) ↔ front (ROLES del mockup). */
  const ROL_FRONT = {
    GERENTE: 'gerente', ADMINISTRADOR: 'admin', ANFITRION: 'anfitrion', JEFE_PISO: 'jefepiso', CRUPIER: 'crupier',
    MARKETING: 'marketing', CAJERO: 'cajero', TALENTO_HUMANO: 'rrhh', CUMPLIMIENTO: 'cumplimiento',
  };
  const ROL_API = Object.fromEntries(Object.entries(ROL_FRONT).map(([api, front]) => [front, api]));

  /* ---------- 1. Token: sessionStorage, o memoria si el almacenamiento falla ---------- */
  const CLAVE = 'pitboss.token';
  let enMemoria = null;
  const leer = () => { try { return sessionStorage.getItem(CLAVE) || enMemoria; } catch { return enMemoria; } };
  const guardar = t => { enMemoria = t; try { sessionStorage.setItem(CLAVE, t); } catch { /* queda en memoria */ } };
  const borrar = () => { enMemoria = null; try { sessionStorage.removeItem(CLAVE); } catch { /* nada que borrar */ } };

  const enLinea = () => PitBossApi.estado.online;
  const ultimo = iso => {
    if (!iso) return 'Nunca';
    const d = fechaApi(iso);
    return `${dos(d.getDate())}/${dos(d.getMonth() + 1)} · ${hora(d)}`;
  };
  const docDe = s => s && (s.apiDoc || docApi(s.doc)) || null;
  const sidDe = documento => documento ? S.staff.find(s => docDe(s) === documento)?.id : undefined;

  /** Pasa un UsuarioDto de la API a una fila de S.users (la crea si no existe) y la devuelve. */
  function integrar(u) {
    let x = S.users.find(y => y.email === u.correo);
    if (!x) { x = { id: 'ua' + u.id }; S.users.push(x); }
    Object.assign(x, {
      name: u.nombre,
      email: u.correo,
      role: ROL_FRONT[u.rol],
      st: u.activo ? 'Activo' : 'Inactivo',
      apiId: u.id,
      last: ultimo(u.ultimoIngreso),
      sid: sidDe(u.empleadoDocumento),
    });
    return x;
  }

  /* ---------- 3. Entrar con el usuario de la API ---------- */
  function entrar(u) {
    const x = integrar(u);
    S.user = { uid: x.id };
    S.view = ROLES[x.role].home;
    S.login.err = '';
    S.mode = 'erp';
  }

  /* ---------- 2. Ganchos del núcleo ---------- */
  PitBossApi.encabezados = () => leer() ? { Authorization: 'Bearer ' + leer() } : {};
  PitBossApi.alNoAutorizado = () => {
    if (!S.user) return;
    borrar();
    S.user = null;
    S.modal = null;
    render();
    toast('Tu sesión terminó. Inicia sesión de nuevo.', true);
  };
  PitBossApi.antesDeSincronizar = async () => {
    if (!leer()) { render(); return false; }   // render: cambia al login con correo de la API
    try {
      const yo = await pedir('GET', '/auth/yo');
      entrar(yo.usuario);
      return true;
    } catch {
      borrar();
      render();
      return false;
    }
  };

  /* ---------- 4. Login: correo escrito (con sugerencias) en vez del selector de usuarios ---------- */
  const loginVistaLocal = loginView;
  loginView = function () {
    if (!enLinea()) return loginVistaLocal();
    const L = S.login, correo = L.correo ?? userById(L.uid)?.email ?? '';
    return `
<div class="login">
  <section class="login-brand">${logoSVG(170)}<h1>PitBoss ERP</h1><p>Un solo sistema para todo el piso del casino</p>
    <button class="side-btn" data-act="playerMode" style="margin-top:10px">Abrir la app del jugador (móvil)</button>
    <small>Universidad de Cundinamarca · Ingeniería de Software II · Prototipo funcional · Sprint 2</small></section>
  <section class="login-form"><form class="login-card form" data-form="login" autocomplete="off">
    <div><h2>Iniciar sesión</h2><p class="hint" style="margin:4px 0 0">Cuenta del personal con verificación en dos pasos</p></div>
    <label class="field"><span>Correo</span><input class="in" id="lg-mail" list="lg-users" inputmode="email" autocapitalize="off" data-bind="login.correo" value="${esc(correo)}"></label>
    <datalist id="lg-users">${S.users.map(x => `<option value="${esc(x.email)}">${esc(x.name)}</option>`).join('')}</datalist>
    <label class="field"><span>Contraseña</span><input class="in" id="lg-pass" type="password" data-bind="login.pass" value="${esc(L.pass)}"></label>
    <label class="field"><span>Código de la app autenticadora (TOTP · 6 dígitos)</span><input class="in num" id="lg-code" inputmode="numeric" maxlength="6" data-bind="login.code" value="${esc(L.code)}" style="letter-spacing:.5em;font-size:20px;font-weight:700"></label>
    ${L.err ? `<p class="err-msg">${esc(L.err)}</p>` : ''}
    <button class="btn primary block" type="submit">Ingresar</button>
    <p class="demo">Demo local: contraseña Casino2026 y código 482913. Con 5 intentos errados la cuenta se bloquea 15 minutos. Cada rol solo ve sus módulos.</p>
  </form></section>
</div>`;
  };

  /* ---------- 5. FORM.login contra POST /api/auth/login ---------- */
  const loginLocal = FORM.login;
  FORM.login = function (fd) {
    if (!enLinea()) return loginLocal(fd);
    const correo = (S.login.correo ?? userById(S.login.uid)?.email ?? '').trim();
    pedir('POST', '/auth/login', { correo, contrasena: S.login.pass, codigo: S.login.code })
      .then(async r => {
        guardar(r.token);
        entrar(r.usuario);
        await PitBossApi.sincronizar();
        if (r.usuario.debeCambiarContrasena) { S.modal = { type: 'cambiarContrasena' }; render(); }
      })
      .catch(e => { S.login.err = e.message; render(); });
  };

  /* ---------- 6. Cerrar sesión ---------- */
  const logoutLocal = ACT.logout;
  ACT.logout = function (...args) {
    borrar();
    return logoutLocal.apply(this, args);
  };

  /* ---------- 7. Carga "usuarios" (orden 70): S.users queda igual a la base ---------- */
  registrarCarga('usuarios', 70,
    () => pedirSi('usuarios', 'GET', '/usuarios'),
    lista => {
      const filas = lista.map(integrar);
      // Quita las filas que la API no tiene (por ejemplo, una cuenta que el prototipo creó y la API rechazó).
      S.users = S.users.filter(x => filas.includes(x) || x.id === S.user?.uid);
    });

  /* ---------- 8. Acciones de "Usuarios y roles" ---------- */
  const datosUnaVez = 'Entrega estos datos una sola vez a la persona. Debe cambiar la contraseña en su primer ingreso.';

  conectar(FORM, 'userNew', {
    antes: fd => {
      const s = staff(fd.get('sid'));
      return {
        n: S.users.length,
        nombre: fd.get('name').trim(),
        correo: fd.get('email').trim().toLowerCase(),
        rol: ROL_API[fd.get('role')],
        empleadoDocumento: docDe(s),
      };
    },
    exito: ctx => S.users.length > ctx.n,
    guardar: async ctx => {
      const r = await pedir('POST', '/usuarios', {
        nombre: ctx.nombre, correo: ctx.correo, rol: ctx.rol, empleadoDocumento: ctx.empleadoDocumento,
      });
      const d = r.datos;
      S.modal = {
        type: 'csv', wide: true, title: 'Usuario creado', sub: datosUnaVez,
        csv: `Correo: ${d.usuario.correo}\nContraseña temporal: ${d.contrasenaTemporal}\nConfigurar app autenticadora: ${d.uriTotp}`,
      };
    },
  });

  conectar(FORM, 'userRole', {
    antes: fd => {
      const x = userById(fd.get('id'));
      return x && x.apiId ? { id: x.id, apiId: x.apiId, role: fd.get('role') } : null;
    },
    exito: ctx => userById(ctx.id).role === ctx.role,
    guardar: ctx => pedir('PUT', `/usuarios/${ctx.apiId}/rol`, { rol: ROL_API[ctx.role] }),
  });

  conectar(ACT, 'resetPass', {
    antes: id => { const x = userById(id); return x && x.apiId ? { apiId: x.apiId } : null; },
    guardar: async ctx => {
      const r = await pedir('POST', `/usuarios/${ctx.apiId}/restablecer`);
      S.modal = {
        type: 'csv', wide: true, title: 'Contraseña restablecida', sub: datosUnaVez,
        csv: `Correo: ${r.datos.usuario.correo}\nContraseña temporal: ${r.datos.contrasenaTemporal}`,
      };
    },
  });

  conectar(ACT, 'userOff', {
    antes: id => { const x = userById(id); return x && x.apiId ? { apiId: x.apiId } : null; },
    guardar: ctx => pedir('POST', `/usuarios/${ctx.apiId}/desactivar`),
  });

  conectar(ACT, 'userOn', {
    antes: id => { const x = userById(id); return x && x.apiId ? { apiId: x.apiId } : null; },
    guardar: ctx => pedir('POST', `/usuarios/${ctx.apiId}/reactivar`),
  });

  /* ---------- 9. Cambiar la propia contraseña ---------- */
  MODALS.cambiarContrasena = m => `<h2>Cambia tu contraseña</h2>
    <p class="sub">Tu contraseña es temporal. Elige una nueva para seguir usando el ERP.</p>
    <form class="form" data-form="cambiarContrasena" autocomplete="off">
      <label class="field"><span>Contraseña actual</span><input class="in" type="password" name="actual" id="cc-actual"></label>
      <label class="field"><span>Contraseña nueva</span><input class="in" type="password" name="nueva" id="cc-nueva"></label>
      <label class="field"><span>Confirmar contraseña nueva</span><input class="in" type="password" name="nueva2" id="cc-nueva2"></label>
      <p class="hint" style="margin:0">Mínimo 8 caracteres, una mayúscula, un número y un carácter especial.</p>
      ${m.err ? `<p class="err-msg">${esc(m.err)}</p>` : ''}
      <div class="mfoot"><button type="button" class="btn sec" data-act="closeModal">Después</button><button class="btn primary" type="submit">Cambiar contraseña</button></div>
    </form>`;

  FORM.cambiarContrasena = function (fd) {
    const actual = fd.get('actual'), nueva = fd.get('nueva'), nueva2 = fd.get('nueva2');
    if (nueva !== nueva2) { S.modal.err = 'Las contraseñas no coinciden.'; return; }
    pedir('POST', '/auth/cambiar-contrasena', { actual, nueva })
      .then(() => { S.modal = null; render(); toast('Contraseña actualizada.'); })
      .catch(e => { if (S.modal) { S.modal.err = e.message; render(); } });
  };
})();
