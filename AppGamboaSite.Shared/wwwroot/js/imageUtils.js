window.imageUtils = {
    loadImageOnCanvas: function (canvasId, imageDataUrl, maxWidth, maxHeight) {
        return new Promise((resolve, reject) => {

            try {
                const canvas = document.getElementById(canvasId);
                if (!canvas) {
                    const error = "Canvas não encontrado: " + canvasId;
                    reject(error);
                    return;
                }

                const ctx = canvas.getContext('2d');
                if (!ctx) {
                    const error = "Contexto 2D não disponível";
                    reject(error);
                    return;
                }

                const img = new Image();

                img.onload = function () {
                    try {

                        let width = img.width;
                        let height = img.height;

                        if (width === 0 || height === 0) {
                            throw new Error("Imagem com dimensões zero");
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
                        ctx.clearRect(0, 0, width, height);
                        ctx.drawImage(img, 0, 0, width, height);

                        resolve({ width: width, height: height });

                    } catch (error) {
                        reject(error.message);
                    }
                };

                img.onerror = function (e) {
                    const error = "Falha ao carregar imagem: " + (e.message || "erro desconhecido");
                    reject(error);
                };

                img.src = imageDataUrl;

            } catch (error) {
                reject(error.message);
            }
        });
    },

    pickColorFromCanvas: function (canvasId, clientX, clientY) {

        try {
            const canvas = document.getElementById(canvasId);
            if (!canvas) {
                return null;
            }

            const ctx = canvas.getContext('2d', { willReadFrequently: true });
            const rect = canvas.getBoundingClientRect();

            const x = clientX - rect.left;
            const y = clientY - rect.top;

            const scaleX = canvas.width / rect.width;
            const scaleY = canvas.height / rect.height;
            const canvasX = Math.floor(x * scaleX);
            const canvasY = Math.floor(y * scaleY);

            if (canvasX < 0 || canvasX >= canvas.width || canvasY < 0 || canvasY >= canvas.height) {
                return null;
            }

            const pixel = ctx.getImageData(canvasX, canvasY, 1, 1).data;
            const color = "#" + ((1 << 24) + (pixel[0] << 16) + (pixel[1] << 8) + pixel[2]).toString(16).slice(1).toUpperCase();
            return color;

        } catch (error) {
            return null;
        }
    },

    processImageDirect: function (canvasId, targetColorHex, tolerance) {

        return new Promise((resolve, reject) => {
            try {
                const useLocalStorage = typeof localStorage !== 'undefined';

                if (useLocalStorage) {
                    localStorage.removeItem('imageProcessingResult');
                    localStorage.removeItem('imageProcessingError');
                    localStorage.setItem('imageProcessingStatus', 'processing');
                }

                let normalizedColor = targetColorHex;
                if (typeof targetColorHex === 'string') {
                    normalizedColor = targetColorHex.trim();
                    if (!normalizedColor.startsWith('#')) {
                        normalizedColor = '#' + normalizedColor;
                    }

                    if (normalizedColor.length === 9) {
                        normalizedColor = normalizedColor.substring(0, 7);
                    }
                }

                if (!normalizedColor || typeof normalizedColor !== 'string' || !normalizedColor.match(/^#[0-9A-Fa-f]{6}$/)) {
                    throw new Error(`Cor inválida: ${targetColorHex} (normalizada: ${normalizedColor}). Deve estar no formato #RRGGBB`);
                }

                if (typeof tolerance !== 'number' || tolerance < 0 || tolerance > 255) {
                    throw new Error("Tolerância inválida: " + tolerance);
                }

                const canvas = document.getElementById(canvasId);
                if (!canvas) {
                    throw new Error("Canvas não encontrado: " + canvasId);
                }

                const ctx = canvas.getContext('2d', { willReadFrequently: true });
                if (!ctx) {
                    throw new Error("Contexto 2D não disponível");
                }

                const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
                const data = imageData.data;

                if (!data || data.length === 0) {
                    throw new Error("Dados da imagem não disponíveis ou vazios");
                }

                const targetR = parseInt(normalizedColor.slice(1, 3), 16);
                const targetG = parseInt(normalizedColor.slice(3, 5), 16);
                const targetB = parseInt(normalizedColor.slice(5, 7), 16);

                let pixelsProcessed = 0;

                for (let i = 0; i < data.length; i += 4) {
                    const r = data[i];
                    const g = data[i + 1];
                    const b = data[i + 2];

                    if (Math.abs(r - targetR) <= tolerance &&
                        Math.abs(g - targetG) <= tolerance &&
                        Math.abs(b - targetB) <= tolerance) {
                        data[i + 3] = 0;
                        pixelsProcessed++;
                    }
                }

                ctx.putImageData(imageData, 0, 0);

                const processedDataUrl = canvas.toDataURL('image/png');

                if (!processedDataUrl || processedDataUrl.length < 100) {
                    throw new Error("Falha ao gerar dataURL - resultado muito pequeno");
                }

                if (useLocalStorage) {
                    localStorage.setItem('imageProcessingResult', processedDataUrl);
                    localStorage.setItem('imageProcessingStatus', 'completed');
                    localStorage.setItem('imageProcessingPixels', pixelsProcessed.toString());

                    resolve("SUCCESS");
                } else {
                    resolve(processedDataUrl);
                }

            } catch (error) {

                if (typeof localStorage !== 'undefined') {
                    localStorage.setItem('imageProcessingError', error.message);
                    localStorage.setItem('imageProcessingStatus', 'error');
                }

                reject(error.message);
            }
        });
    },

    clearProcessingResult: function () {
        if (typeof localStorage !== 'undefined') {
            localStorage.removeItem('imageProcessingResult');
            localStorage.removeItem('imageProcessingError');
            localStorage.removeItem('imageProcessingStatus');
            localStorage.removeItem('imageProcessingPixels');
        }
    },

    downloadFromLocalStorage: function () {
        try {
            const stored = localStorage.getItem('imageProcessingResult');
            if (stored && stored.startsWith('data:image')) {
                const link = document.createElement('a');
                link.href = stored;
                link.download = 'emergencia_imagem_sem_fundo.png';
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
                console.log("JS: Download iniciado.");
                return 'Download de emergência realizado via JS';
            } else {
                console.warn("JS: Nenhum resultado válido encontrado no localStorage.");
                return 'Nenhum resultado encontrado no localStorage para download';
            }
        } catch (e) {
            return 'Erro no JS: ' + e.message;
        }
    }
};

window.blazorImageUtils = window.imageUtils;