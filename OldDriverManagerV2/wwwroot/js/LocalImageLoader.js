function byteToUrl(blob)
{
    var myBlob = new Blob([blob], { type: "image/png" });
    return (window.URL || window.webkitURL || window || {}).createObjectURL(myBlob);
}

function revokeUrl(url)
{
    (window.URL || window.webkitURL || window || {}).revokeObjectURL(url);
}