// Ejercicio 3 (variante) y Taller 4: inventario de fichas. Responsable: Felipe Antonio Cortés Hernández
const { InventarioFichasService } = require('../src/inventario/inventarioFichasService');
const { StockInsuficienteError } = require('../src/errores');

describe('InventarioFichasService', () => {
  let service;
  let mockRepo;
  const ficha = (codigo, stock) => ({ codigo, denominacion: 25000, categoria: 'Verde', stock });

  beforeEach(() => {
    mockRepo = {
      existsByCode: jest.fn().mockResolvedValue(false),
      findByCode: jest.fn(),
      save: jest.fn(async (f) => f),
      addMovement: jest.fn(),
    };
    service = new InventarioFichasService(mockRepo);
  });

  it('registra una ficha válida y llama a save una sola vez', async () => {
    const f = await service.registrar({ codigo: 'FICHA-50K', denominacion: 50000, categoria: 'Naranja', stockInicial: 2600 });
    expect(f.stock).toBe(2600);
    expect(mockRepo.save).toHaveBeenCalledTimes(1);
  });

  it('rechaza un código de ficha duplicado', async () => {
    mockRepo.existsByCode.mockResolvedValue(true);
    await expect(service.registrar({ codigo: 'FICHA-50K', denominacion: 50000, categoria: 'Naranja', stockInicial: 1 })).rejects.toThrow('El código de ficha ya está registrado');
    expect(mockRepo.save).not.toHaveBeenCalled();
  });

  it.each([
    ['El stock inicial no puede ser negativo', { codigo: 'FICHA-1M', denominacion: 1000000, categoria: 'Morado', stockInicial: -5 }],
    ['La categoría es obligatoria', { codigo: 'FICHA-2K', denominacion: 2000, categoria: '', stockInicial: 100 }],
    ['El código de ficha no es válido', { codigo: 'X-1', denominacion: 2000, categoria: 'Azul', stockInicial: 1 }],
  ])('rechaza datos inválidos: "%s"', async (mensaje, datos) => {
    await expect(service.registrar(datos)).rejects.toThrow(mensaje);
  });

  it.each([
    ['FICHA-5K', 10, 3, 7],
    ['FICHA-10K', 5, 5, 0],
  ])('retiro de %s: stock %i, retiro %i -> final %i', async (codigo, stock, retiro, final) => {
    mockRepo.findByCode.mockResolvedValue(ficha(codigo, stock));
    const f = await service.retirar(codigo, retiro);
    expect(f.stock).toBe(final);
    expect(mockRepo.addMovement).toHaveBeenCalledWith(expect.objectContaining({ tipo: 'SALIDA', cantidad: retiro }));
  });

  it('lanza StockInsuficienteError y no cambia el stock', async () => {
    mockRepo.findByCode.mockResolvedValue(ficha('FICHA-25K', 2));
    await expect(service.retirar('FICHA-25K', 5)).rejects.toBeInstanceOf(StockInsuficienteError);
    expect(mockRepo.save).not.toHaveBeenCalled();
    expect(mockRepo.addMovement).not.toHaveBeenCalled();
  });

  it('registra una entrada y su movimiento', async () => {
    mockRepo.findByCode.mockResolvedValue(ficha('FICHA-25K', 50));
    const f = await service.registrarEntrada('FICHA-25K', 10);
    expect(f.stock).toBe(60);
    expect(mockRepo.addMovement).toHaveBeenCalledWith(expect.objectContaining({ tipo: 'ENTRADA', stockFinal: 60 }));
  });

  it('rechaza cantidades no positivas', async () => {
    await expect(service.registrarEntrada('FICHA-25K', 0)).rejects.toThrow('La cantidad debe ser mayor que cero');
    await expect(service.retirar('FICHA-25K', -1)).rejects.toThrow('La cantidad debe ser mayor que cero');
  });
});
