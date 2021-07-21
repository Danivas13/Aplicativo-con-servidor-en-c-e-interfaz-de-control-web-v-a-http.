# Imagen/video (Demostración):
## Titulo:
### Aplicativo con servidor en c# e interfaz de control web vía http.
# Objetivo:
El objetivo fue elaborar un servidor utilizando los recursos de conexión que ofrece el c# nativo sin dll adicionales que estableciera una conexión con un puerto (5001 o 80) y presentara una interfaz web que consiste en archivos HTML, Css y JavaScript para que sea mas amigable y fácil de modificar sin necesidad de compilar.
Esta idea toma como inspiración WhatsApp web en donde uno de nuestros dispositivos contiene un servidor el cual puede ser accedido desde el navegador de otro dispositivo. Entre los objetivos que se definieron fueron: 
-  Que fuese escrito completamente en c# sin necesidad de otros lenguajes como php
- Capacidad de establecer la conexión y procesar solicitudes post y get.

La aplicación para un fin practico es capaz recibir un enlace de YouTube mediante una solicitud http enviada mediante JavaScript al presionar un botón de la interfaz web, que posteriormente es recibida por el servidor el cual desencadena la descarga del video en formato mp4 que posteriormente es convertido a mp3 usando la librería ffmpeg eliminando el video y reduciendo su peso considerablemente, todo esto mientras retro alimenta la interfaz web casi que en tiempo real (sin necesidad de refrescar el navegador) con información sobre el estado actual de la cola de descarga y conversión. También es capaz de mantener actualizada la lista de archivos de mp3 que es encuentras dentro de la carpeta música y presentarla en la interfaz web. 

## Construido con:
### Lenguajes:
C#, JavaScript, HTML, Css
### Frameworks:
JQuery
### Librerías:
Ffmpeg (Para conversión de mp4 a mp3)

- Para probarlo debera addicionar los dll de ffmpeg, ffmpeg32.dll y ffmpeg32.dll en la carpeta debug.

- Please add the ffmpeg32.dll and ffmpeg32.dll to the debug folder.
