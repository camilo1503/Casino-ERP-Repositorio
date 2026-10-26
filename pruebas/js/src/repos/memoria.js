/** Repositorios en memoria: reemplazan a Azure SQL en las pruebas (no hay base de datos real). */
const copia = (x) => (x ? JSON.parse(JSON.stringify(x)) : x);

class RepoJugadores {
  constructor() { this.datos = new Map(); }
  async existsByDocument(doc) { return this.datos.has(doc); }
  async findByDocument(doc) { return copia(this.datos.get(doc)); }
  async save(j) { this.datos.set(j.documento, copia(j)); return copia(j); }
  async count() { return this.datos.size; }
}

class RepoMesas {
  constructor() { this.datos = new Map(); }
  async existsByNumber(n) { return this.datos.has(n); }
  async findByNumber(n) { return copia(this.datos.get(n)); }
  async save(m) { this.datos.set(m.numero, copia(m)); return copia(m); }
}

class RepoCompras {
  constructor() { this.datos = []; }
  async save(c) { this.datos.push(copia(c)); return copia(c); }
}

class RepoOfertas {
  constructor() { this.datos = []; }
  async countOffers(doc, fecha) { return this.datos.filter((o) => o.documento === doc && o.fecha === fecha).length; }
  async save(o) { this.datos.push(copia(o)); return copia(o); }
}

class RepoRellenos {
  constructor(saldoBoveda = 0) { this.datos = new Map(); this.saldo = saldoBoveda; }
  async findById(id) { return copia(this.datos.get(id)); }
  async save(r) { this.datos.set(r.id, copia(r)); return copia(r); }
  async getVaultBalance() { return this.saldo; }
  async updateVaultBalance(v) { this.saldo = v; }
}

class RepoFichas {
  constructor() { this.datos = new Map(); this.movimientos = []; }
  async existsByCode(c) { return this.datos.has(c); }
  async findByCode(c) { return copia(this.datos.get(c)); }
  async save(f) { this.datos.set(f.codigo, copia(f)); return copia(f); }
  async addMovement(m) { this.movimientos.push(copia(m)); }
}

module.exports = { RepoJugadores, RepoMesas, RepoCompras, RepoOfertas, RepoRellenos, RepoFichas };
