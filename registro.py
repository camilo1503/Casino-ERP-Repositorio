def registrar_usuario(nombre, correo, password):
    print(f"Registrando usuario: {nombre}")

def validar_correo(correo):
    if "@" not in correo or "." not in correo:
        raise ValueError("Correo invalido")
    return True
