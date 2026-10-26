// Plantilla CRUD de la sección 1 llevada a Jest: perfiles de jugador. Responsable: Daniel Camilo Ruiz Rodríguez
const { JugadorService, edad } = require('../src/perfiles/jugadorService');

describe('JugadorService - registro y borrado lógico', () => {
  let service;
  let mockRepo;
  const maria = { documento: '1020456789', nombre: 'María Fernanda López', correo: 'Maria.Lopez@correo.com', fechaNacimiento: '1990-05-14' };

  beforeEach(() => {
    mockRepo = { existsByDocument: jest.fn().mockResolvedValue(false), findByDocument: jest.fn(), save: jest.fn(async (j) => j) };
    service = new JugadorService(mockRepo, () => '2026-10-26');
  });

  it('deberiaCrearJugadorConCorreoValido: el repositorio recibe el jugador en estado ACTIVO', async () => {
    await service.registrar(maria);
    expect(mockRepo.save).toHaveBeenCalledTimes(1);
    expect(mockRepo.save.mock.calls[0][0]).toMatchObject({ documento: '1020456789', correo: 'maria.lopez@correo.com', estado: 'ACTIVO' });
  });

  it('debería rechazar un documento duplicado', async () => {
    mockRepo.existsByDocument.mockResolvedValue(true);
    await expect(service.registrar(maria)).rejects.toThrow('El documento ya está registrado');
    expect(mockRepo.save).not.toHaveBeenCalled();
  });

  it('debería rechazar a un menor de edad', async () => {
    await expect(service.registrar({ ...maria, documento: '1099000111', fechaNacimiento: '2010-03-01' })).rejects.toThrow('El jugador debe ser mayor de 18 años');
  });

  it.each(['', ' ', 'sin-arroba.com', 'usuario@'])('debería rechazar el correo inválido "%s"', async (correo) => {
    await expect(service.registrar({ ...maria, correo })).rejects.toThrow('Formato de correo inválido');
  });

  it('debería calcular la edad cumplida a la fecha', () => {
    expect(edad('2008-10-26', '2026-10-26')).toBe(18);
    expect(edad('2008-10-27', '2026-10-26')).toBe(17);
  });

  it('no debería permitir cambiar el documento al actualizar', async () => {
    mockRepo.findByDocument.mockResolvedValue({ ...maria, estado: 'ACTIVO' });
    await expect(service.actualizar('1020456789', { documento: '999' })).rejects.toThrow('El documento no se puede modificar');
  });

  it('debería desactivar con motivo y conservar el registro (borrado lógico)', async () => {
    mockRepo.findByDocument.mockResolvedValue({ ...maria, estado: 'ACTIVO' });
    const j = await service.desactivar('1020456789', 'Solicitud del titular');
    expect(j.estado).toBe('INACTIVO');
    expect(j.documento).toBe('1020456789');
  });

  it('debería exigir motivo para autoexcluir', async () => {
    await expect(service.autoexcluir('1020456789', '')).rejects.toThrow('El motivo es obligatorio');
  });
});
