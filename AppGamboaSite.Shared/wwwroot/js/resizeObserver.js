window.createResizeObserver = (dotNetRef) => {
    const handleResize = () => {
        dotNetRef.invokeMethodAsync('OnWindowResize', window.innerWidth);
    };

    window.addEventListener('resize', handleResize);
    handleResize();

    return {};
};
