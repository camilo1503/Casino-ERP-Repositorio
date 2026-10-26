// Ejercicio 2 llevado a Jest (equivalente al @CsvSource de JUnit) y regla del Ejercicio 4. Responsable: Iván Felipe León Pinilla
const { CompraFichasService } = require('../src/caja/compraFichasService');

describe('CompraFichasService', () => {
  let service;
  let mockRepo;

  beforeEach(() => {
    mockRepo = { save: jest.fn(async (c) => c) };
    service = new CompraFichasService(mockRepo);
  });

  it.each([
    [25000, 4, 100000],
    [100000, 3, 300000],
    [5000, 20, 100000],
  ])('%i x %i = %i', (denominacion, cantidad, total) => {
    expect(service.calcularTotal(denominacion, cantidad)).toBe(total);
  });

  it.each([[0], [-2], [1.5]])('rechaza la cantidad %p', (cantidad) => {
    expect(() => service.calcularTotal(25000, cantidad)).toThrow('La cantidad debe ser mayor que cero');
  });

  it('rechaza una denominación que no existe', () => {
    expect(() => service.calcularTotal(7000, 1)).toThrow('Denominación no válida');
  });

  it('rechaza una compra con monto cero y no la guarda', async () => {
    await expect(service.registrarCompra('1020456789', 0)).rejects.toThrow('El monto debe ser mayor que cero');
    expect(mockRepo.save).not.toHaveBeenCalled();
  });

  it.each([
    [100000, 'APROBADA'],
    [4999999, 'APROBADA'],
    [5000000, 'EN_REVISION'],
    [8750000, 'EN_REVISION'],
  ])('una compra de %i queda %s', async (monto, estado) => {
    const compra = await service.registrarCompra('1020456789', monto);
    expect(compra.estado).toBe(estado);
  });
});
