# SpecFlow (proyectos heredados)

SpecFlow se retiró el 31 de diciembre de 2024 y Reqnroll es su sucesor. Las step definitions de este proyecto
funcionan en SpecFlow cambiando solo dos cosas:

1. Paquetes: `dotnet add package SpecFlow.NUnit` y `dotnet add package SpecFlow.Plus.LivingDocPlugin`
   en lugar de `Reqnroll.NUnit`.
2. Espacio de nombres: `using TechTalk.SpecFlow;` en lugar de `using Reqnroll;`.

El archivo `reqnroll.json` se reemplaza por `specflow.json` con el mismo contenido (`"language": {"feature": "es"}`).
