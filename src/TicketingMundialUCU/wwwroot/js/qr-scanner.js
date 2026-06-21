window.ticketQrScanner = (() => {
  let stream = null;
  let timerId = null;
  let video = null;
  let dotNetRef = null;
  let detector = null;
  let detected = false;
  let processing = false;

  async function start(videoElement, callbackRef) {
    stop();
    dotNetRef = callbackRef;

    if (!window.isSecureContext) {
      await notifyError("La camara solo esta disponible con HTTPS o desde localhost. En celulares no funciona desde una IP local con HTTP.");
      return false;
    }

    if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
      await notifyError("El navegador no permite acceder a la camara. Usa HTTPS o el ingreso manual.");
      return false;
    }

    if (!("BarcodeDetector" in window)) {
      await notifyError("Este navegador no soporta lectura QR desde camara. Usa el ingreso manual.");
      return false;
    }

    video = videoElement;
    detected = false;

    try {
      detector = new BarcodeDetector({ formats: ["qr_code"] });
      stream = await navigator.mediaDevices.getUserMedia({
        audio: false,
        video: {
          facingMode: { ideal: "environment" },
          width: { ideal: 1280 },
          height: { ideal: 720 }
        }
      });

      video.srcObject = stream;
      video.setAttribute("playsinline", "true");
      await video.play();

      timerId = window.setInterval(scanFrame, 250);
      return true;
    } catch (error) {
      stop();
      dotNetRef = callbackRef;
      await notifyError(getCameraErrorMessage(error));
      return false;
    }
  }

  async function scanFrame() {
    if (!video || !detector || !dotNetRef || detected || processing ||
        video.readyState < HTMLMediaElement.HAVE_CURRENT_DATA) {
      return;
    }

    processing = true;

    try {
      const codes = await detector.detect(video);
      processing = false;

      if (!dotNetRef || detected) {
        return;
      }

      const qr = codes.find(code => code.rawValue);

      if (!qr) {
        return;
      }

      detected = true;
      await dotNetRef.invokeMethodAsync("OnQrDetected", qr.rawValue);
    } catch (error) {
      processing = false;
      const callbackRef = dotNetRef;
      stop();
      if (callbackRef !== null) {
        await callbackRef.invokeMethodAsync(
          "OnScannerError",
          error && error.message ? error.message : "No se pudo leer el codigo QR."
        );
      }
    }
  }

  async function notifyError(message) {
    if (dotNetRef !== null) {
      await dotNetRef.invokeMethodAsync("OnScannerError", message);
    }
  }

  function getCameraErrorMessage(error) {
    if (!error || !error.name) {
      return "No se pudo acceder a la camara. Usa el ingreso manual.";
    }

    if (error.name === "NotAllowedError" || error.name === "SecurityError") {
      return "El permiso de camara fue rechazado o bloqueado por el navegador.";
    }

    if (error.name === "NotFoundError" || error.name === "OverconstrainedError") {
      return "No se encontro una camara disponible en este dispositivo.";
    }

    if (error.name === "NotReadableError") {
      return "La camara esta siendo usada por otra aplicacion.";
    }

    return "No se pudo acceder a la camara. Usa el ingreso manual.";
  }

  function stop() {
    if (timerId !== null) {
      window.clearInterval(timerId);
      timerId = null;
    }

    if (stream !== null) {
      for (const track of stream.getTracks()) {
        track.stop();
      }
      stream = null;
    }

    if (video !== null) {
      video.pause();
      video.srcObject = null;
      video = null;
    }

    dotNetRef = null;
    detector = null;
    detected = false;
    processing = false;
  }

  return { start, stop };
})();
