window.blazorImageUtils = {
    imageProcessorWorker: null,
    dotNetObjectReference: null, // Para armazenar a referência .NET

    loadImageOnCanvas: function (canvasId, imageDataUrl, maxWidth, maxHeight) {
        return new Promise((resolve, reject) => {
            const canvas = document.getElementById(canvasId);
            if (!canvas) {
                console.error("JS: Elemento canvas não encontrado: " + canvasId);
                reject("Elemento canvas não encontrado: " + canvasId);
                return;
            }
            const ctx = canvas.getContext('2d');
            if (!ctx) {
                console.error("JS: Falha ao obter contexto 2D do canvas: " + canvasId);
                reject("Falha ao obter contexto 2D do canvas.");
                return;
            }
            const img = new Image();
            img.onload = function () {
                let width = img.width;
                let height = img.height;

                if (width === 0 || height === 0) {
                    console.error("JS: Imagem carregada com dimensões zero.");
                    reject("Imagem carregada com dimensões zero.");
                    return;
                }

                if (width > height) {
                    if (width > maxWidth) {
                        height = Math.round(height * (maxWidth / width));
                        width = maxWidth;
                    }
                } else {
                    if (height > maxHeight) {
                        width = Math.round(width * (maxHeight / height));
                        height = maxHeight;
                    }
                }
                canvas.width = width;
                canvas.height = height;
                ctx.clearRect(0, 0, canvas.width, canvas.height);
                ctx.drawImage(img, 0, 0, width, height);
                console.log("JS: Imagem carregada no canvas:", canvasId, "W:", width, "H:", height);
                resolve({ width: width, height: height });
            };
            img.onerror = function (e) {
                console.error("JS: Falha ao carregar imagem no objeto Image:", e);
                reject("Falha ao carregar imagem. Verifique o formato ou a integridade do arquivo.");
            };
            img.src = imageDataUrl;
        });
    },

    pickColorFromCanvas: function (canvasId, clientX, clientY) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return null;
        const ctx = canvas.getContext('2d', { willReadFrequently: true });
        const rect = canvas.getBoundingClientRect();

        const x = clientX - rect.left;
        const y = clientY - rect.top;

        const scaleX = canvas.width / rect.width;
        const scaleY = canvas.height / rect.height;
        const canvasX = Math.floor(x * scaleX);
        const canvasY = Math.floor(y * scaleY);

        if (canvasX < 0 || canvasX >= canvas.width || canvasY < 0 || canvasY >= canvas.height) {
            console.warn("JS: Clique fora da área de desenho do canvas.");
            return null;
        }

        try {
            const pixel = ctx.getImageData(canvasX, canvasY, 1, 1).data;
            return "#" + ((1 << 24) + (pixel[0] << 16) + (pixel[1] << 8) + pixel[2]).toString(16).slice(1).toUpperCase();
        } catch (e) {
            console.error("JS: Erro ao selecionar cor: ", e);
            return null;
        }
    },

    initImageProcessor: function (dotNetRef) {
        if (typeof (Worker) !== "undefined") {
            if (window.blazorImageUtils.imageProcessorWorker == null) {
                console.log("JS: Inicializando Worker de Processamento de Imagem...");
                window.blazorImageUtils.imageProcessorWorker = new Worker('js/image-processor.worker.js');
                window.blazorImageUtils.dotNetObjectReference = dotNetRef;

                window.blazorImageUtils.imageProcessorWorker.onmessage = function (e) {
                    console.log('JS: Mensagem recebida do worker');
                    if (window.blazorImageUtils.dotNetObjectReference) {
                        if (e.data.success && e.data.processedData) {
                            window.blazorImageUtils.dotNetObjectReference.invokeMethodAsync(
                                'HandleWorkerSuccess',
                                Array.from(new Uint8ClampedArray(e.data.processedData)),
                                e.data.width,
                                e.data.height
                            );
                        } else if (e.data.error) {
                            window.blazorImageUtils.dotNetObjectReference.invokeMethodAsync('HandleWorkerError', e.data.error);
                        }
                    }
                };

                window.blazorImageUtils.imageProcessorWorker.onerror = function (e) {
                    console.error('JS: Erro no worker:', e.message, e.filename, e.lineno);
                    if (window.blazorImageUtils.dotNetObjectReference) {
                        window.blazorImageUtils.dotNetObjectReference.invokeMethodAsync('HandleWorkerError', 'Erro crítico no worker: ' + e.message);
                    }
                };

                console.log("JS: Worker inicializado.");
            } else {
                console.log("JS: Worker já existe. Atualizando referência .NET se necessário.");
                window.blazorImageUtils.dotNetObjectReference = dotNetRef;
            }
            return true;
        } else {
            console.error("JS: Web Workers não são suportados por este navegador.");
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('HandleWorkerError', 'Web Workers não são suportados.');
            }
            return false;
        }
    },

    processImageWithWorker: function (canvasId, targetColorHex, tolerance) {
        if (!window.blazorImageUtils.imageProcessorWorker) {
            console.error("JS: Worker não inicializado.");
            if (window.blazorImageUtils.dotNetObjectReference) {
                window.blazorImageUtils.dotNetObjectReference.invokeMethodAsync('HandleWorkerError', 'Worker não inicializado ao tentar processar.');
            }
            return;
        }

        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.error("JS: Elemento canvas não encontrado para processamento.");
            if (window.blazorImageUtils.dotNetObjectReference) {
                window.blazorImageUtils.dotNetObjectReference.invokeMethodAsync('HandleWorkerError', 'Canvas não encontrado para processamento.');
            }
            return;
        }
        const ctx = canvas.getContext('2d', { willReadFrequently: true });
        const originalImageData = ctx.getImageData(0, 0, canvas.width, canvas.height);

        console.log("JS: Enviando dados da imagem para o worker para processamento.");
        window.blazorImageUtils.imageProcessorWorker.postMessage({
            rawData: originalImageData.data.buffer,
            width: canvas.width,
            height: canvas.height,
            targetColorHex: targetColorHex,
            tolerance: tolerance,
            action: 'removeBackground'
        }, [originalImageData.data.buffer]);
    },

    putImageDataOnCanvasAndGetDataURL: function (canvasId, dataArray, width, height) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.error("JS: Canvas não encontrado para colocar dados processados.");
            return null;
        }

        const ctx = canvas.getContext('2d');
        const imageData = ctx.createImageData(width, height);
        imageData.data.set(new Uint8ClampedArray(dataArray));
        ctx.putImageData(imageData, 0, 0);
        return canvas.toDataURL('image/png');
    },

    terminateWorker: function () {
        if (window.blazorImageUtils.imageProcessorWorker) {
            window.blazorImageUtils.imageProcessorWorker.terminate();
            window.blazorImageUtils.imageProcessorWorker = null;
            window.blazorImageUtils.dotNetObjectReference = null;
            console.log("JS: Worker terminado.");
        }
    }
};
