// wwwroot/js/imageWorker.js
self.onmessage = function (e) {
    console.log('WORKER: Mensagem recebida do script principal. Dados:', e.data); // LOG ADICIONADO
    const { rawImageData, width, height, targetColorHex, tolerance, action } = e.data;

    if (action === 'removeBackground') {
        if (!rawImageData || !width || !height) {
            console.error('WORKER: Dados brutos da imagem, largura ou altura não fornecidos.');
            self.postMessage({ error: 'Dados brutos da imagem, largura ou altura não fornecidos pelo worker.' });
            return;
        }

        try {
            const data = new Uint8ClampedArray(rawData);
            const processedData = processPixelData(data, targetColorHex, tolerance);
            console.log('WORKER: Processamento concluído. Enviando mensagem de sucesso de volta.'); // LOG ADICIONADO
            self.postMessage({
                processedData: processedData.buffer, 
                width: width,
                height: height,
                success: true
            }, [processedData.buffer]);
        } catch (error) {
            console.error('WORKER: Erro ao processar imagem', error);
            console.log('WORKER: Enviando mensagem de ERRO de volta.'); // LOG ADICIONADO
            self.postMessage({ error: error.message || 'Erro desconhecido no worker' });
        }
    }
};

function processPixelData(imageData, targetColorHex, tolerance) {
    // imageData aqui é um objeto ImageData { data: Uint8ClampedArray, width, height }
    const data = imageData.data;
    const targetR = parseInt(targetColorHex.slice(1, 3), 16);
    const targetG = parseInt(targetColorHex.slice(3, 5), 16);
    const targetB = parseInt(targetColorHex.slice(5, 7), 16);

    for (let i = 0; i < data.length; i += 4) {
        const r = data[i];
        const g = data[i + 1];
        const b = data[i + 2];

        if (Math.abs(r - targetR) <= tolerance &&
            Math.abs(g - targetG) <= tolerance &&
            Math.abs(b - targetB) <= tolerance) {
            data[i + 3] = 0;
        }

        
    }
    return data; // Retorna o objeto ImageData modificado
}