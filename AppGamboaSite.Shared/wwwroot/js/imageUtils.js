// Versão compatível - mantém função antiga + adiciona localStorage
window.imageUtils = {
    loadImageOnCanvas: function (canvasId, imageDataUrl, maxWidth, maxHeight) {
        return new Promise((resolve, reject) => {
            console.log("JS: [loadImageOnCanvas] Iniciando carregamento...");

            try {
                const canvas = document.getElementById(canvasId);
                if (!canvas) {
                    const error = "Canvas não encontrado: " + canvasId;
                    console.error("JS: [loadImageOnCanvas]", error);
                    reject(error);
                    return;
                }

                const ctx = canvas.getContext('2d');
                if (!ctx) {
                    const error = "Contexto 2D não disponível";
                    console.error("JS: [loadImageOnCanvas]", error);
                    reject(error);
                    return;
                }

                const img = new Image();

                img.onload = function () {
                    try {
                        console.log("JS: [loadImageOnCanvas] Imagem carregada:", img.width, "x", img.height);

                        let width = img.width;
                        let height = img.height;

                        if (width === 0 || height === 0) {
                            throw new Error("Imagem com dimensões zero");
                        }

                        // Calcular dimensões redimensionadas
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

                        console.log("JS: [loadImageOnCanvas] Redimensionando para:", width, "x", height);

                        canvas.width = width;
                        canvas.height = height;
                        ctx.clearRect(0, 0, width, height);
                        ctx.drawImage(img, 0, 0, width, height);

                        console.log("JS: [loadImageOnCanvas] Sucesso!");
                        resolve({ width: width, height: height });

                    } catch (error) {
                        console.error("JS: [loadImageOnCanvas] Erro ao desenhar:", error);
                        reject(error.message);
                    }
                };

                img.onerror = function (e) {
                    const error = "Falha ao carregar imagem: " + (e.message || "erro desconhecido");
                    console.error("JS: [loadImageOnCanvas]", error);
                    reject(error);
                };

                img.src = imageDataUrl;

            } catch (error) {
                console.error("JS: [loadImageOnCanvas] Erro inesperado:", error);
                reject(error.message);
            }
        });
    },

    pickColorFromCanvas: function (canvasId, clientX, clientY) {
        console.log("JS: [pickColorFromCanvas] Iniciando seleção de cor...");

        try {
            const canvas = document.getElementById(canvasId);
            if (!canvas) {
                console.error("JS: [pickColorFromCanvas] Canvas não encontrado");
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

            console.log("JS: [pickColorFromCanvas] Coordenadas:", canvasX, canvasY);

            if (canvasX < 0 || canvasX >= canvas.width || canvasY < 0 || canvasY >= canvas.height) {
                console.warn("JS: [pickColorFromCanvas] Clique fora dos limites");
                return null;
            }

            const pixel = ctx.getImageData(canvasX, canvasY, 1, 1).data;
            const color = "#" + ((1 << 24) + (pixel[0] << 16) + (pixel[1] << 8) + pixel[2]).toString(16).slice(1).toUpperCase();
            console.log("JS: [pickColorFromCanvas] Cor selecionada:", color);
            return color;

        } catch (error) {
            console.error("JS: [pickColorFromCanvas] Erro:", error);
            return null;
        }
    },

    // Versão que funciona com localStorage (nova)
    processImageDirect: function (canvasId, targetColorHex, tolerance) {
        console.log("JS: [processImageDirect] Iniciando processamento...");
        console.log("JS: [processImageDirect] Parâmetros - Cor:", targetColorHex, "Tolerância:", tolerance);

        return new Promise((resolve, reject) => {
            try {
                // Verificar se deve usar localStorage
                const useLocalStorage = typeof localStorage !== 'undefined';

                if (useLocalStorage) {
                    // Limpar resultado anterior
                    localStorage.removeItem('imageProcessingResult');
                    localStorage.removeItem('imageProcessingError');
                    localStorage.setItem('imageProcessingStatus', 'processing');
                }

                // CORREÇÃO: Normalizar cor para remover canal alpha se presente
                let normalizedColor = targetColorHex;
                if (typeof targetColorHex === 'string') {
                    // Remover espaços e garantir que comece com #
                    normalizedColor = targetColorHex.trim();
                    if (!normalizedColor.startsWith('#')) {
                        normalizedColor = '#' + normalizedColor;
                    }

                    // Se tem 8 caracteres (inclui alpha), remover os 2 últimos
                    if (normalizedColor.length === 9) {
                        normalizedColor = normalizedColor.substring(0, 7);
                        console.log("JS: [processImageDirect] Cor normalizada de", targetColorHex, "para", normalizedColor);
                    }
                }

                // Validar parâmetros
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

                console.log("JS: [processImageDirect] Canvas encontrado:", canvas.width, "x", canvas.height);
                console.log("JS: [processImageDirect] Cor final:", normalizedColor);

                // Obter dados da imagem
                const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
                const data = imageData.data;

                if (!data || data.length === 0) {
                    throw new Error("Dados da imagem não disponíveis ou vazios");
                }

                console.log("JS: [processImageDirect] Dados obtidos - Total pixels:", data.length / 4);

                // Extrair componentes RGB da cor alvo (usando cor normalizada)
                const targetR = parseInt(normalizedColor.slice(1, 3), 16);
                const targetG = parseInt(normalizedColor.slice(3, 5), 16);
                const targetB = parseInt(normalizedColor.slice(5, 7), 16);

                console.log("JS: [processImageDirect] RGB alvo:", targetR, targetG, targetB);

                let pixelsProcessed = 0;

                // Processar cada pixel
                for (let i = 0; i < data.length; i += 4) {
                    const r = data[i];
                    const g = data[i + 1];
                    const b = data[i + 2];

                    // Verificar se o pixel está dentro da tolerância
                    if (Math.abs(r - targetR) <= tolerance &&
                        Math.abs(g - targetG) <= tolerance &&
                        Math.abs(b - targetB) <= tolerance) {
                        data[i + 3] = 0; // Tornar transparente
                        pixelsProcessed++;
                    }
                }

                console.log("JS: [processImageDirect] Pixels processados:", pixelsProcessed);

                // Aplicar a imagem processada de volta ao canvas
                ctx.putImageData(imageData, 0, 0);
                console.log("JS: [processImageDirect] Dados aplicados de volta ao canvas");

                // Obter o dataURL da imagem processada
                const processedDataUrl = canvas.toDataURL('image/png');

                if (!processedDataUrl || processedDataUrl.length < 100) {
                    throw new Error("Falha ao gerar dataURL - resultado muito pequeno");
                }

                if (useLocalStorage) {
                    // Usar localStorage (retorna "SUCCESS")
                    localStorage.setItem('imageProcessingResult', processedDataUrl);
                    localStorage.setItem('imageProcessingStatus', 'completed');
                    localStorage.setItem('imageProcessingPixels', pixelsProcessed.toString());

                    console.log("JS: [processImageDirect] Resultado salvo no localStorage, tamanho:", processedDataUrl.length);
                    resolve("SUCCESS");
                } else {
                    // Retornar dataURL diretamente (modo compatibilidade)
                    console.log("JS: [processImageDirect] Retornando dataURL diretamente, tamanho:", processedDataUrl.length);
                    resolve(processedDataUrl);
                }

            } catch (error) {
                console.error("JS: [processImageDirect] Erro:", error);

                if (typeof localStorage !== 'undefined') {
                    localStorage.setItem('imageProcessingError', error.message);
                    localStorage.setItem('imageProcessingStatus', 'error');
                }

                reject(error.message);
            }
        });
    },

    // Funções para localStorage (novas - opcionais)
    getProcessingResult: function () {
        if (typeof localStorage === 'undefined') {
            return { status: 'not_supported' };
        }

        const status = localStorage.getItem('imageProcessingStatus');
        const result = localStorage.getItem('imageProcessingResult');
        const error = localStorage.getItem('imageProcessingError');
        const pixels = localStorage.getItem('imageProcessingPixels');

        return {
            status: status,
            result: result,
            error: error,
            pixelsProcessed: pixels ? parseInt(pixels) : 0
        };
    },

    clearProcessingResult: function () {
        if (typeof localStorage !== 'undefined') {
            localStorage.removeItem('imageProcessingResult');
            localStorage.removeItem('imageProcessingError');
            localStorage.removeItem('imageProcessingStatus');
            localStorage.removeItem('imageProcessingPixels');
        }
    }
};

// Manter compatibilidade com nomes antigos
window.blazorImageUtils = window.imageUtils;