// Ejercicio 3 (Jest): Jorge Alcides Díaz Lozano · Taller 1 (retiro de mesas): Daniel Camilo Ruiz Rodríguez
const { MesaService } = require('../src/mesas/mesaService');

describe('MesaService - registro de mesas (Ejercicio 3)', () => {
  let service;
  let mockRepo;
  const valida = { numero: 'M-19', juego: 'Blackjack', apuestaMinima: 25000, apuestaMaxima: 500000 };

  beforeEach(() => {
    mockRepo = {
      existsByNumber: jest.fn(),
      findByNumber: jest.fn(),
      save: jest.fn(async (m) => m),
    };
    service = new MesaService(mockRepo);
  });

  afterEach(() => jest.clearAllMocks());

  it('debería registrar una mesa válida y llamar a save una sola vez', async () => {
    mockRepo.existsByNumber.mockResolvedValue(false);
    const mesa = await service.registrar(valida);
    expect(mesa.estado).toBe('CERRADA');
    expect(mockRepo.save).toHaveBeenCalledTimes(1);
    expect(mockRepo.save).toHaveBeenCalledWith(expect.objectContaining({ numero: 'M-19', estado: 'CERRADA' }));
  });

  it('debería rechazar una mesa con número duplicado sin guardar nada', async () => {
    mockRepo.existsByNumber.mockResolvedValue(true);
    await expect(service.registrar(valida)).rejects.toThrow('El número de mesa ya está registrado');
    expect(mockRepo.save).not.toHaveBeenCalled();
  });

  it.each([
    ['El número de mesa debe tener el formato M-00', { ...valida, numero: '19' }],
    ['El juego es obligatorio', { ...valida, juego: '' }],
    ['La apuesta mínima debe ser mayor que cero', { ...valida, apuestaMinima: 0 }],
    ['La apuesta mínima debe ser menor que la máxima', { ...valida, apuestaMinima: 500000, apuestaMaxima: 100000 }],
  ])('debería rechazar datos inválidos: "%s"', async (mensaje, datos) => {
    mockRepo.existsByNumber.mockResolvedValue(false);
    await expect(service.registrar(datos)).rejects.toThrow(mensaje);
    expect(mockRepo.save).not.toHaveBeenCalled();
  });
});

describe('MesaService - actualizar y retirar (Taller 1)', () => {
  let service;
  let mockRepo;

  beforeEach(() => {
    mockRepo = { existsByNumber: jest.fn(), findByNumber: jest.fn(), save: jest.fn(async (m) => m) };
    service = new MesaService(mockRepo);
  });

  it('debería retirar una mesa cerrada y dejarla en estado RETIRADA', async () => {
    mockRepo.findByNumber.mockResolvedValue({ numero: 'M-13', juego: 'Póker', estado: 'CERRADA' });
    const mesa = await service.retirar('M-13', 'Paño dañado sin reparación');
    expect(mesa.estado).toBe('RETIRADA');
    expect(mockRepo.save).toHaveBeenCalledWith(expect.objectContaining({ estado: 'RETIRADA', motivoRetiro: 'Paño dañado sin reparación' }));
  });

  it('no debería retirar una mesa abierta', async () => {
    mockRepo.findByNumber.mockResolvedValue({ numero: 'M-01', juego: 'Blackjack', estado: 'ABIERTA' });
    await expect(service.retirar('M-01', 'Cambio de distribución')).rejects.toThrow('No se puede retirar una mesa abierta');
    expect(mockRepo.save).not.toHaveBeenCalled();
  });

  it('no debería retirar una mesa sin motivo', async () => {
    mockRepo.findByNumber.mockResolvedValue({ numero: 'M-13', estado: 'CERRADA' });
    await expect(service.retirar('M-13', '  ')).rejects.toThrow('El motivo es obligatorio');
  });

  it('debería actualizar los límites de una mesa existente', async () => {
    mockRepo.findByNumber.mockResolvedValue({ numero: 'M-05', estado: 'CERRADA', apuestaMinima: 50000, apuestaMaxima: 800000 });
    const mesa = await service.actualizarLimites('M-05', 50000, 1000000);
    expect(mesa.apuestaMaxima).toBe(1000000);
  });

  it('no debería actualizar una mesa retirada', async () => {
    mockRepo.findByNumber.mockResolvedValue({ numero: 'M-10', estado: 'RETIRADA' });
    await expect(service.actualizarLimites('M-10', 10000, 20000)).rejects.toThrow('La mesa está retirada');
  });

  it('debería informar cuando la mesa no existe', async () => {
    mockRepo.findByNumber.mockResolvedValue(undefined);
    await expect(service.consultar('M-99')).rejects.toThrow('Mesa no encontrada');
  });
});
