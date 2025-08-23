window.getImageSize = async (dataurl) => {
    return await new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => resolve({width: img.naturalWidth, height: img.naturalHeight});
        img.onerror = reject;
        img.src = dataurl;
    });
}