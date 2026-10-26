// Taller 2 (Jest, it.each): ofertas de retención. Responsable: Jorge Alcides Díaz Lozano
const { RetencionService } = require('../src/retencion/retencionService');
const { pesos } = require('../src/dinero');

describe('RetencionService - descuento del 20 % con tope (Taller 2)', () => {
  let service;
  let mockRepo;

  beforeEach(() => {
    mockRepo = { save: jest.fn(async (o) => o), countOffers: jest.fn().mockResolvedValue(0) };
    service = new RetencionService(mockRepo);
  });

  it.each([
    [500000, 'Descuento de $100.000'],
    [2500000, 'Descuento de $200.000'],
  ])('compra de %i pesos -> "%s" y se guarda una vez', async (monto, resultado) => {
    const descuento = await service.aplicarDescuento('1020456789', monto);
    expect(`Descuento de ${pesos(descuento)}`).toBe(resultado);
    expect(mockRepo.save).toHaveBeenCalledTimes(1);
  });

  it.each([[0], [-1000]])('compra de %i pesos -> "Monto inválido" y no llama a save', async (monto) => {
    await expect(service.aplicarDescuento('1020456789', monto)).rejects.toThrow('Monto inválido');
    expect(mockRepo.save).not.toHaveBeenCalled();
  });
});

describe('RetencionService - reglas de juego responsable', () => {
  let service;
  let mockRepo;
  const jugador = (puntos, estado = 'ACTIVO') => ({ documento: '1020456789', puntos, estado });

  beforeEach(() => {
    mockRepo = { save: jest.fn(async (o) => o), countOffers: jest.fn().mockResolvedValue(0) };
    service = new RetencionService(mockRepo);
  });

  it.each([
    [900, 'DESCUENTO_FICHAS'],
    [3200, 'DESCUENTO_FICHAS'],
    [8450, 'FICHAS_BONO'],
    [15200, 'BONO_RECLAMABLE'],
  ])('con %i puntos sugiere %s', async (puntos, tipo) => {
    const oferta = await service.evaluarSenal({ jugador: jugador(puntos), minutosSesion: 95, senal: 'Quiere retirarse' });
    expect(oferta.tipo).toBe(tipo);
    expect(oferta.estado).toBe('EMITIDA');
  });

  it('el bono reclamable vence a los 30 días', async () => {
    const oferta = await service.evaluarSenal({ jugador: jugador(15200), minutosSesion: 60, senal: 'Me retiro' });
    expect(oferta.vence).toBe('2026-11-25');
    expect(oferta.valor).toBe(100000);
  });

  it.each([
    ['AUTOEXCLUIDO', 95, 0, 'No se ofrece retención: juego responsable'],
    ['INACTIVO', 95, 0, 'No se ofrece retención: juego responsable'],
    ['ACTIVO', 22, 0, 'La sesión debe durar al menos 30 minutos'],
    ['ACTIVO', 95, 1, 'El jugador ya recibió una oferta hoy'],
  ])('bloquea la oferta (estado %s, %i min, %i ofertas hoy)', async (estado, minutos, ofertasHoy, mensaje) => {
    mockRepo.countOffers.mockResolvedValue(ofertasHoy);
    await expect(service.evaluarSenal({ jugador: jugador(8450, estado), minutosSesion: minutos, senal: 'Quiere retirarse' })).rejects.toThrow(mensaje);
    expect(mockRepo.save).not.toHaveBeenCalled();
  });

  it('rechaza una señal sin perfil de jugador', async () => {
    await expect(service.evaluarSenal({ jugador: null, minutosSesion: 40 })).rejects.toThrow('Jugador sin perfil registrado');
  });
});
