# Propuesta de Diseño: Sistema de Temas Dinámicos en WPF

Este documento detalla la propuesta para implementar un sistema de cambio de temas visuales en tiempo real dentro del cliente *Letter Clash*, permitiendo a los jugadores elegir entre el estilo monocromático OLED actual y paletas de colores alternativas (como temas colorizados de estilo Cyberpunk, retro-consola verde, o bosque clásico pixelado).

---

## 1. Justificación y Objetivos

### Flexibilidad Visual
Aunque el estilo oscuro monocromático es el tema principal de *Letter Clash*, algunos usuarios pueden preferir temas con más contraste cromático o con paletas que recuerden a otros juegos clásicos.

### Personalización sin Sobrecarga
Un sistema de temas bien implementado permite cambiar toda la paleta de colores de la aplicación al instante sin requerir que la ventana del juego se reinicie o recargue sus datos.

### Escalabilidad del Diseño
Centraliza todos los colores (fondos, textos, bordes, estados de hover/press) en diccionarios de recursos compartidos (`ResourceDictionary`), lo que facilita agregar nuevos temas en el futuro simplemente creando un archivo XAML adicional sin tocar el código fuente de las vistas.

---

## 2. Funcionamiento en WPF (Arquitectura)

En WPF, los recursos de diseño se pueden evaluar de manera estática (`StaticResource`) o dinámica (`DynamicResource`). 

1. **StaticResource**: Se resuelve una sola vez al cargar la vista. Si el color cambia en memoria, el control conserva el color original.
2. **DynamicResource**: Se resuelve en tiempo de ejecución cada vez que se requiere pintar el control. Si el recurso asociado en la colección `Application.Current.Resources` se modifica, el control se repinta automáticamente.

Al migrar todos nuestros estilos en `App.xaml` o vistas individuales para que usen `DynamicResource` en lugar de colores fijos, dejamos la aplicación lista para soportar temas.

---

## 3. Implementación Propuesta

### A. Creación de Diccionarios de Temas
Los diccionarios se guardarán en una carpeta nueva en el cliente: `Themes/`.

#### Tema Monocromático (`Themes/ThemeMonochrome.xaml`):
```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <!-- Colores Base -->
    <SolidColorBrush x:Key="PageBgBrush" Color="#09090B"/>
    <SolidColorBrush x:Key="CardBgBrush" Color="#121214"/>
    <SolidColorBrush x:Key="BorderBrush" Color="#2D2D30"/>
    <SolidColorBrush x:Key="GridLineBrush" Color="#15151A"/>

    <!-- Textos -->
    <SolidColorBrush x:Key="TextPrimaryBrush" Color="#FFFFFF"/>
    <SolidColorBrush x:Key="TextSecondaryBrush" Color="#A1A1AA"/>

    <!-- Botón Primario -->
    <SolidColorBrush x:Key="PrimaryButtonBgBrush" Color="#E4E4E7"/>
    <SolidColorBrush x:Key="PrimaryButtonTextBrush" Color="#09090B"/>
</ResourceDictionary>
```

#### Tema Verde Terminal / Retro-Hack (`Themes/ThemeTerminalGreen.xaml`):
```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <!-- Colores Base -->
    <SolidColorBrush x:Key="PageBgBrush" Color="#031003"/>
    <SolidColorBrush x:Key="CardBgBrush" Color="#052005"/>
    <SolidColorBrush x:Key="BorderBrush" Color="#00FF66"/>
    <SolidColorBrush x:Key="GridLineBrush" Color="#082A08"/>

    <!-- Textos -->
    <SolidColorBrush x:Key="TextPrimaryBrush" Color="#00FF66"/>
    <SolidColorBrush x:Key="TextSecondaryBrush" Color="#33FF99"/>

    <!-- Botón Primario -->
    <SolidColorBrush x:Key="PrimaryButtonBgBrush" Color="#00FF66"/>
    <SolidColorBrush x:Key="PrimaryButtonTextBrush" Color="#031003"/>
</ResourceDictionary>
```

### B. Aplicación en los Controles
Por ejemplo, los botones y textboxes del juego usarán las llaves del tema:
```xml
<Style x:Key="ModernPrimaryButton" TargetType="Button">
    <Setter Property="Background" Value="{DynamicResource PrimaryButtonBgBrush}"/>
    <Setter Property="Foreground" Value="{DynamicResource PrimaryButtonTextBrush}"/>
    <!-- ... triggers ... -->
</Style>
```

### C. Clase de Gestión de Temas (ThemeManager)
Se creará un servicio sencillo para cambiar el tema en tiempo de ejecución modificando los diccionarios fusionados globales:

```csharp
using System;
using System.Linq;
using System.Windows;

namespace LetterClashClient.Services {
    public static class ThemeManager {
        public static void SetTheme(string themeName) {
            var appResources = Application.Current.Resources;
            
            // Buscar si ya hay un diccionario de tema cargado
            var existingTheme = appResources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Themes/Theme"));
                
            if (existingTheme != null) {
                appResources.MergedDictionaries.Remove(existingTheme);
            }
            
            // Cargar el nuevo diccionario
            var newTheme = new ResourceDictionary {
                Source = new Uri($"/LetterClashClient;component/Themes/Theme{themeName}.xaml", UriKind.RelativeOrAbsolute)
            };
            
            appResources.MergedDictionaries.Add(newTheme);
        }
    }
}
```

---

## 4. Interfaz de Usuario (Ajustes)

En `GUISettingsView.xaml`, se añadirá una sección de **"Personalización de Tema"** con un ComboBox:
- **Monocromático OLED** (Predeterminado)
- **Terminal Verde**
- **Bosque Pixelado** (Marrón y Verde)
- **Cyberpunk** (Azul y Rosa)

Cuando el usuario cambie la selección del ComboBox, se invocará `ThemeManager.SetTheme(...)` y se guardará la preferencia del usuario en el archivo de configuración local (`App.config` o similar) para mantener el tema seleccionado en futuros inicios de la aplicación.

---

## 5. Plan de Ejecución
Esta propuesta se implementará en la **fase de pulido y ajustes**, una vez que todas las vistas del juego hayan sido rediseñadas bajo la estructura base de estilos y fuentes Geist.
