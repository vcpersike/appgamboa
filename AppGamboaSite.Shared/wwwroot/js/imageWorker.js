// Worker para processamento de imagem
self.onmessage = function (e) {
    console.log('WORKER: Mensagem recebida do script principal:', e.data);

    const { imageData, targetColorHex, tolerance, action } = e.data;

    if (action === 'removeBackground') {
        if (!imageData || !imageData.data || !imageData.width || !imageData.height) {
            console.error('WORKER: Dados da imagem não fornecidos corretamente.');
            self.postMessage({
                success: false,
                error: 'Dados da imagem não fornecidos corretamente pelo worker.'
            });
            return;
        }

        try {
            console.log('WORKER: Iniciando processamento da imagem...');
            const processedData = processPixelData(imageData, targetColorHex, tolerance);

            // Criar canvas no worker para gerar dataURL
            const canvas = new OffscreenCanvas(processedData.width, processedData.height);
            const ctx = canvas.getContext('2d');

            const newImageData = ctx.createImageData(processedData.width, processedData.height);
            newImageData.data.set(processedData.data);
            ctx.putImageData(newImageData, 0, 0);

            // Converter para blob e depois para dataURL
            canvas.convertToBlob({ type: 'image/png' }).then(blob => {
                const reader = new FileReader();
                reader.onload = function () {
                    console.log('WORKER: Processamento concluído com sucesso.');
                    self.postMessage({
                        success: true,
                        dataUrl: reader.result
                    });
                };
                reader.onerror = function () {
                    console.error('WORKER: Erro ao converter blob para dataURL');
                    self.postMessage({
                        success: false,
                        error: 'Erro ao converter resultado para formato de imagem'
                    });
                };
                reader.readAsDataURL(blob);
            }).catch(error => {
                console.error('WORKER: Erro ao converter canvas para blob:', error);
                self.postMessage({
                    success: false,
                    error: 'Erro ao gerar imagem final: ' + error.message
                });
            });

        } catch (error) {
            console.error('WORKER: Erro ao processar imagem:', error);
            self.postMessage({
                success: false,
                error: error.message || 'Erro desconhecido no worker'
            });
        }
    }
};

function processPixelData(imageData, targetColorHex, tolerance) {
    console.log('WORKER: Processando pixels...');

    // Converter array de volta para Uint8ClampedArray
    const data = new Uint8ClampedArray(imageData.data);
    const width = imageData.width;
    const height = imageData.height;

    // Extrair componentes RGB da cor alvo
    const targetR = parseInt(targetColorHex.slice(1, 3), 16);
    const targetG = parseInt(targetColorHex.slice(3, 5), 16);
    const targetB = parseInt(targetColorHex.slice(5, 7), 16);

    console.log(`WORKER: Cor alvo RGB(${targetR}, ${targetG}, ${targetB}), Tolerância: ${tolerance}`);

    let pixelsProcessed = 0;

    // Processar cada pixel
    for (let i = 0; i < data.length; i += 4) {
        const r = data[i];
        const g = data[i + 1];
        const b = data[i + 2];

        // Verificar se o pixel está dentro da tolerância da cor alvo
        if (Math.abs(r - targetR) <= tolerance &&
            Math.abs(g - targetG) <= tolerance &&
            Math.abs(b - targetB) <= tolerance) {
            data[i + 3] = 0; // Tornar transparente (alpha = 0)
            pixelsProcessed++;
        }
    }

    console.log(`WORKER: ${pixelsProcessed} pixels tornados transparentes`);

    return {
        data: data,
        width: width,
        height: height
    };
}