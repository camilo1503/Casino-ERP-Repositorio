"""Ejercicio 6: genera reporte-pruebas.html con iconos de Font Awesome a partir de los resultados reales.

Uso (desde la carpeta pitboss-pruebas):  python reporte/generar_reporte.py
Lee: evidencias/jest-results.json, cucumber.json, TEST-junit-jupiter.xml,
     cucumber-jvm.json (mvn test) y dotnet/reqnroll.trx (dotnet test)
Responsable: Felipe Antonio Cortés Hernández
"""
import html
import json
import re
import xml.etree.ElementTree as ET
from datetime import date
from pathlib import Path

RAIZ = Path(__file__).resolve().parent.parent
EVID = RAIZ / "evidencias"

MODULOS = {
    "perfiles": ("Perfiles de jugador", "fa-id-card"),
    "mesas": ("Catálogo de mesas", "fa-dice"),
    "caja": ("Caja / compra de fichas", "fa-cash-register"),
    "retencion": ("Retención de jugadores", "fa-hand-holding-heart"),
    "rellenos": ("Rellenos de fichas", "fa-coins"),
    "inventario": ("Inventario de fichas", "fa-boxes-stacked"),
}
ESTADOS = {
    "aprobado": ("Aprobado", "fa-circle-check", "ok"),
    "fallido": ("Fallido", "fa-circle-xmark", "bad"),
    "pendiente": ("Pendiente", "fa-clock", "warn"),
}
HERRAMIENTAS = {
    "Jest": "fa-brands fa-node-js",
    "JUnit 5": "fa-brands fa-java",
    "Cucumber-JS": "fa-solid fa-list-check",
    "Cucumber-JVM": "fa-brands fa-java",
    "Reqnroll (SpecFlow)": "fa-brands fa-microsoft",
}


def modulo_de(texto):
    t = texto.lower()
    claves = {"jugador": "perfiles", "perfiles": "perfiles", "mesa": "mesas", "compra": "caja", "caja": "caja",
              "retencion": "retencion", "relleno": "rellenos", "inventario": "inventario", "ficha": "inventario"}
    for k, v in claves.items():
        if k in t:
            return v
    return "inventario"


def leer_jest():
    filas = []
    datos = json.loads((EVID / "jest-results.json").read_text(encoding="utf-8"))
    for suite in datos["testResults"]:
        mod = modulo_de(Path(suite["name"]).stem)
        for a in suite["assertionResults"]:
            estado = {"passed": "aprobado", "failed": "fallido"}.get(a["status"], "pendiente")
            filas.append(("Jest", mod, " › ".join(a["ancestorTitles"]), a["title"], estado))
    return filas


def leer_cucumber():
    filas = []
    for feat in json.loads((EVID / "cucumber.json").read_text(encoding="utf-8")):
        mod = modulo_de(Path(feat["uri"]).stem)
        for esc in feat["elements"]:
            if esc.get("type") == "background":
                continue
            st = [p["result"]["status"] for p in esc["steps"]]
            estado = "aprobado" if all(s == "passed" for s in st) else "fallido" if "failed" in st else "pendiente"
            filas.append(("Cucumber-JS", mod, feat["name"], esc["name"], estado))
    return filas


def leer_junit():
    filas = []
    arbol = ET.parse(EVID / "TEST-junit-jupiter.xml")
    for tc in arbol.getroot().iter("testcase"):
        clase = tc.get("classname", "")
        mod = clase.split(".")[3] if clase.count(".") >= 3 else modulo_de(clase)
        salida = tc.findtext("system-out") or ""
        m = re.search(r"display-name: (.*)", salida)
        nombre = m.group(1).strip() if m else tc.get("name")
        estado = "fallido" if tc.find("failure") is not None or tc.find("error") is not None else "pendiente" if tc.find("skipped") is not None else "aprobado"
        filas.append(("JUnit 5", mod, clase.split(".")[-1], nombre, estado))
    return filas


def leer_cucumber_jvm():
    """Resultados reales de `mvn test` (RunCucumberTest) copiados de java/target/cucumber.json."""
    filas = []
    for feat in json.loads((EVID / "cucumber-jvm.json").read_text(encoding="utf-8")):
        mod = modulo_de(Path(feat["uri"].split(":")[-1]).stem)
        for esc in feat["elements"]:
            if esc.get("type") == "background":
                continue
            st = [p["result"]["status"] for p in esc.get("before", []) + esc["steps"] + esc.get("after", [])]
            estado = "aprobado" if all(s == "passed" for s in st) else "fallido" if "failed" in st else "pendiente"
            filas.append(("Cucumber-JVM", mod, feat["name"], esc["name"], estado))
    return filas


NOMBRES_REQNROLL = {
    "RetiroDentroDelStockDisponible": "Retiro dentro del stock disponible",
    "RetirarMasFichasDeLasDisponibles": "Retirar más fichas de las disponibles",
}


def leer_reqnroll():
    """Resultados reales de `dotnet test` (logger trx) en evidencias/dotnet/reqnroll.trx."""
    filas = []
    ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
    raiz = ET.parse(EVID / "dotnet" / "reqnroll.trx").getroot()
    for r in raiz.iterfind(".//t:UnitTestResult", ns):
        estado = {"Passed": "aprobado", "Failed": "fallido"}.get(r.get("outcome"), "pendiente")
        nombre = NOMBRES_REQNROLL.get(r.get("testName")) or re.sub(r"(?<!^)(?=[A-Z])", " ", r.get("testName")).capitalize()
        filas.append(("Reqnroll (SpecFlow)", "inventario", "Retiro de fichas de la bóveda", nombre, estado))
    return filas


def icono_estado(e):
    nombre, icono, clase = ESTADOS[e]
    return f'<span class="st {clase}"><i class="fa-solid {icono}" aria-hidden="true"></i> {nombre}</span>'


def icono_modulo(m):
    nombre, icono = MODULOS[m]
    return f'<span class="mod"><i class="fa-solid {icono}" aria-hidden="true"></i> {html.escape(nombre)}</span>'


def construir():
    filas = leer_jest() + leer_junit() + leer_cucumber() + leer_cucumber_jvm() + leer_reqnroll()
    totales = {e: sum(1 for f in filas if f[4] == e) for e in ESTADOS}
    resumen = {}
    for h, m, _, _, e in filas:
        resumen.setdefault((m, h), {"aprobado": 0, "fallido": 0, "pendiente": 0})[e] += 1
    filas_resumen = []
    for (m, h), c in sorted(resumen.items(), key=lambda x: (list(MODULOS).index(x[0][0]), list(HERRAMIENTAS).index(x[0][1]))):
        estado = "fallido" if c["fallido"] else "pendiente" if c["pendiente"] else "aprobado"
        filas_resumen.append(
            f"<tr><td>{icono_modulo(m)}</td><td><i class='{HERRAMIENTAS[h]}' aria-hidden='true'></i> {h}</td>"
            f"<td class='n'>{c['aprobado']}</td><td class='n'>{c['fallido']}</td><td class='n'>{c['pendiente']}</td><td>{icono_estado(estado)}</td></tr>")
    filas_detalle = [
        f"<tr data-mod='{m}'><td>{icono_estado(e)}</td><td>{icono_modulo(m)}</td><td><i class='{HERRAMIENTAS[h]}' aria-hidden='true'></i> {h}</td>"
        f"<td class='g'>{html.escape(g)}</td><td>{html.escape(n)}</td></tr>"
        for h, m, g, n, e in filas]
    botones = "".join(f"<button type='button' data-f='{k}'><i class='fa-solid {v[1]}' aria-hidden='true'></i> {v[0]}</button>" for k, v in MODULOS.items())
    plantilla = (Path(__file__).parent / "plantilla.html").read_text(encoding="utf-8")
    salida = (plantilla
              .replace("{{FECHA}}", date.today().strftime("%d/%m/%Y"))
              .replace("{{TOTAL}}", str(len(filas)))
              .replace("{{OK}}", str(totales["aprobado"]))
              .replace("{{BAD}}", str(totales["fallido"]))
              .replace("{{PEND}}", str(totales["pendiente"]))
              .replace("{{RESUMEN}}", "\n".join(filas_resumen))
              .replace("{{DETALLE}}", "\n".join(filas_detalle))
              .replace("{{BOTONES}}", botones))
    destino = Path(__file__).parent / "reporte-pruebas.html"
    destino.write_text(salida, encoding="utf-8")
    print(f"Reporte generado: {destino} ({len(filas)} pruebas: {totales})")


if __name__ == "__main__":
    construir()
