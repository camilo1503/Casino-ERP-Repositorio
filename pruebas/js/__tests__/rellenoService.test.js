// Plantilla Jest de la sección 3 (aprobación con mock del repositorio) aplicada a rellenos de fichas.
// Responsable: Iván Felipe León Pinilla
const { RellenoService } = require('../src/rellenos/rellenoService');

describe('RellenoService - aprobación', () => {
  let service;
  let mockRepo;

  beforeEach(() => {
    mockRepo = {
      findById: jest.fn(),
      save: jest.fn(),
      getVaultBalance: jest.fn(),
      updateVaultBalance: jest.fn(),
    };
    service = new RellenoService(mockRepo);
  });

  afterEach(() => jest.clearAllMocks());

  it('debería aprobar un relleno dentro del saldo de la bóveda y descontarlo', async () => {
    mockRepo.findById.mockResolvedValue({ id: 'RF-100', mesa: 'M-04', monto: 5000000, estado: 'PENDIENTE' });
    mockRepo.getVaultBalance.mockResolvedValue(10000000);
    mockRepo.save.mockResolvedValue({ id: 'RF-100', estado: 'APROBADO' });
    const relleno = await service.aprobar('RF-100');
    expect(relleno.estado).toBe('APROBADO');
    expect(mockRepo.updateVaultBalance).toHaveBeenCalledWith(5000000);
    expect(mockRepo.save).toHaveBeenCalledTimes(1);
  });

  it('debería rechazar un relleno que excede el saldo de la bóveda', async () => {
    mockRepo.findById.mockResolvedValue({ id: 'RF-101', mesa: 'M-12', monto: 5000000, estado: 'PENDIENTE' });
    mockRepo.getVaultBalance.mockResolvedValue(3000000);
    await expect(service.aprobar('RF-101')).rejects.toThrow('Saldo insuficiente en la bóveda');
    expect(mockRepo.save).not.toHaveBeenCalled();
    expect(mockRepo.updateVaultBalance).not.toHaveBeenCalled();
  });

  it('no debería aprobar un relleno ya aprobado', async () => {
    mockRepo.findById.mockResolvedValue({ id: 'RF-102', monto: 2000000, estado: 'APROBADO' });
    await expect(service.aprobar('RF-102')).rejects.toThrow('El relleno ya está aprobado');
    expect(mockRepo.getVaultBalance).not.toHaveBeenCalled();
  });

  it('no debería aprobar un relleno rechazado ni uno inexistente', async () => {
    mockRepo.findById.mockResolvedValueOnce({ id: 'RF-103', monto: 1000000, estado: 'RECHAZADO' });
    await expect(service.aprobar('RF-103')).rejects.toThrow('Solo se aprueban rellenos pendientes');
    mockRepo.findById.mockResolvedValueOnce(undefined);
    await expect(service.aprobar('RF-999')).rejects.toThrow('Relleno no encontrado');
  });
});
