# EXILION — README

## 🎮 Nombre del Proyecto
**EXILION**  
*Supervivencia · Mundo Abierto · Cooperativo · Pixel Art*

---

## 👥 Integrantes del Grupo
*   **Blanco Agustín**
*   **Diaz Diaz Ignacio**
*   **Muñoz Juan Ignacio**

---

## 📝 Descripción Corta del Videojuego
**EXILION** es un videojuego de supervivencia cooperativo 2D con vista cenital (*top-down*) y estética Pixel Art en el que un grupo de hasta 4 astronautas queda varado en un planeta alienígena hostil tras un accidente catastrófico. El núcleo de la experiencia consiste en gestionar recursos vitales críticos en tiempo real —especialmente los niveles de oxígeno, al carecer el planeta de atmósfera respirable—, además del hambre, la sed y la salud corporal mediante un sistema de daño por partes del cuerpo. Los jugadores deberán explorar un gigantesco mapa generado de forma procedural, recolectar materiales, construir una base operativa autosuficiente y combatir la fauna local para alcanzar el objetivo final: construir una nave de escape y abandonar el planeta.

---

## 🛠️ Tecnologías Principales y Plataformas Objetivo
*   **Lenguaje de Programación:** C# (compatible con .NET 9 o superior)
*   **Framework de Desarrollo:** MonoGame 3.8.4.1 (Cross-Platform Desktop Application)
*   **Arquitectura:** Programación Orientada a Objetos (OOP)
*   **Protocolos de Red:** TCP (para datos de entrega garantizada como chat y gestión de sesiones) y UDP (para datos continuos de posición y entidades en tiempo real) bajo arquitectura Cliente-Servidor.
*   **Plataformas Objetivo:** 
    *   🖥️ **PC (Windows y Linux):** Plataformas prioritarias de ejecución y compilación.

---

## 🔗 Enlace info
Puedes acceder a la propuesta detallada del proyecto, minutas de diseño, diagramas de arquitectura y el Documento de Diseño de Juego (GDD) completo a través del siguiente enlace directo:
👉 **[Wiki del Proyecto EXILION - Propuesta Detallada](https://github.com/Totoo27/EXILION/wiki)** 
👉 **[Video primer prototipo - EXILION]([https://github.com/Totoo27/EXILION/wiki](https://youtu.be/A6OlYRtCE_Q))** 

---

## 🚀 Instrucciones Básicas de Compilación y Ejecución
Sigue estos pasos para clonar el repositorio localmente, compilar el código fuente y ejecutar la instancia del videojuego:

### 📋 Prerrequisitos
Antes de comenzar, asegúrate de tener instalado en tu sistema:
1. **Git** (Control de versiones).
2. **.NET SDK 9.0 o superior** (Necesario para el entorno de compilación de MonoGame).
3. Un IDE compatible con .NET como **Visual Studio** o **Visual Studio Code**.

### 1. Clonar el Repositorio
Abre tu terminal o consola de comandos y ejecuta el siguiente comando para clonar el proyecto:
```bash
git clone https://github.com/Totoo27/EXILION.git
cd EXILION
```

### 2. Restaurar Dependencias de NuGet
Restaura todos los paquetes NuGet asociados a la solución (incluyendo librerías de MonoGame y dependencias de red):
```bash
dotnet restore
```

### 3. Compilar el Proyecto
Para verificar que el código fuente no contiene errores y compilar todo el proyecto:
```bash
dotnet build
```

### 4. Cómo Ejecutar el Proyecto
Para iniciar el juego de manera directa, podés hacerlo de tres formas dependiendo del entorno en el que te encuentres:

#### Opción A: Desde la consola de comandos (Recomendado y más rápido)
Usa la interfaz de línea de comandos de .NET ejecutando el siguiente comando desde la carpeta raíz donde clonaste el proyecto:
```bash
dotnet run --project EXILION.csproj
```

#### Opción B: Desde Visual Studio Code
1. Abre la carpeta del proyecto en VS Code.
2. Abre una terminal integrada (`Ctrl + \``) y ejecuta el comando de la Opción A:
   ```bash
   dotnet run --project EXILION.csjproj
   ```
3. *(Alternativamente)* Si usas la extensión **C# Dev Kit**, presiona **F5** seleccionando la configuración de ejecución para .NET.
