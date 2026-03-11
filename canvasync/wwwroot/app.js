window.getImageSize = async (dataurl) => {
    return await new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => resolve({width: img.naturalWidth, height: img.naturalHeight});
        img.onerror = reject;
        img.src = dataurl;
    });
}

// Host가 저장하지 않고 페이지를 떠나려 할 때 브라우저 경고 표시
window.enableBeforeUnloadWarning = () => {
    window._beforeUnloadHandler = (e) => {
        e.preventDefault();
        // Chrome 구버전 호환용 (현대 브라우저는 무시하고 자체 문구 표시)
        e.returnValue = '';
    };
    window.addEventListener('beforeunload', window._beforeUnloadHandler);
};

window.disableBeforeUnloadWarning = () => {
    if (window._beforeUnloadHandler) {
        window.removeEventListener('beforeunload', window._beforeUnloadHandler);
        window._beforeUnloadHandler = null;
    }
};

window.PdfFileDownload = async (fileName, data) => {
    return await new Promise((resolve, reject) => {
        // alert(data);
        // const byteCharacters = atob(data);
        // const byteNumbers = new Array(byteCharacters.length);
        // for (let i = 0; i < byteCharacters.length; i++) {
        //     byteNumbers[i] = byteCharacters.charCodeAt(i);
        // }
        // const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([data], { type: 'application/octet-stream' });
        const url = URL.createObjectURL(blob);
        const anchorElement = document.createElement('a');
        anchorElement.href = url;
        anchorElement.download = fileName ?? 'downloadedFile';
        anchorElement.style.display = 'none';
        document.body.appendChild(anchorElement);
        anchorElement.click();
        document.body.removeChild(anchorElement);
        URL.revokeObjectURL(url);
    })
}