# PitBoss ERP en Kubernetes (HU-10)

Manifiestos para correr PitBoss ERP en un clúster local con [kind](https://kind.sigs.k8s.io/). Usan la imagen del `Dockerfile` de la raíz.

| Archivo | Qué define |
|---|---|
| `deployment.yaml` | Deployment `pitboss-erp`: 1 réplica, puerto 8080, sondas en `/api/salud`, variables desde el Secret, SQLite en un volumen `emptyDir` |
| `service.yaml` | Service `pitboss-erp` (ClusterIP): puerto 80 → 8080 |
| `kustomization.yaml` | Agrupa los dos recursos para `kubectl apply -k k8s` |

## Requisitos

Docker Desktop, [`kind`](https://kind.sigs.k8s.io/docs/user/quick-start/#installation) y [`kubectl`](https://kubernetes.io/docs/tasks/tools/). En Windows: `winget install Kubernetes.kind Kubernetes.kubectl`.

## Paso a paso (PowerShell, desde la raíz del repo)

```powershell
# 1. Imagen y clúster
docker build -t pitboss-erp:1.0 .
kind create cluster --name pitboss
kind load docker-image pitboss-erp:1.0 --name pitboss

# 2. Secret (NO se sube al repo). La clave debe tener al menos 32 caracteres.
$clave = -join ((48..57)+(65..90)+(97..122) | Get-Random -Count 48 | ForEach-Object {[char]$_})
kubectl create secret generic pitboss-erp-secretos --from-literal=Jwt__Clave=$clave --from-literal=Seguridad__CodigoTotpDemo=482913

# 3. Desplegar y esperar a que el pod esté listo
kubectl apply -k k8s
kubectl rollout status deployment/pitboss-erp
kubectl get pods,svc

# 4. Probar (deja esta terminal abierta y usa otra)
kubectl port-forward svc/pitboss-erp 8081:80
```

En otra terminal: `Invoke-RestMethod http://localhost:8081/api/salud` debe responder `estado: ok`. El front queda en http://localhost:8081.

## Autorreparación y escalado (para la sustentación)

```powershell
# Autorreparación: se borra el pod y Kubernetes crea otro para mantener 1 réplica
kubectl delete pod -l app=pitboss-erp
kubectl get pods -w          # aparece un pod nuevo hasta 1/1 Running (Ctrl+C para salir)

# Escalado
kubectl scale deployment/pitboss-erp --replicas=2
kubectl get pods
```

**Por qué cada réplica tiene datos distintos:** la base es SQLite dentro de un volumen `emptyDir`, que pertenece a un solo pod y se borra con él. Con dos réplicas, cada pod tiene su propia base, y un jugador registrado en uno no aparece en el otro. En producción se escala apuntando todas las réplicas a una base compartida (Azure SQL, `BaseDatos__Proveedor=SqlServer`), como hacen Azure App Service y Vercel.

## Limpieza

```powershell
kind delete cluster --name pitboss
```
