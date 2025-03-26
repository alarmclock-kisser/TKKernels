__kernel void grayscale_filter_static(__global uchar* buffer, const int length) {
    int gid = get_global_id(0);

    // Annahme: Der Buffer enthält RGBA-Daten (4 Bytes pro Pixel)
    int bytesPerPixel = 4;

    // Überprüfen, ob der Index innerhalb des gültigen Bereichs für einen Pixelstart liegt
    if (gid < length / bytesPerPixel) {
        int pixelOffset = gid * bytesPerPixel;

        // Rot-, Grün-, Blau- und Alpha-Komponenten lesen
        uchar red = buffer[pixelOffset];
        uchar green = buffer[pixelOffset + 1];
        uchar blue = buffer[pixelOffset + 2];
        uchar alpha = buffer[pixelOffset + 3];

        // Graustufenwert mit der Standardformel berechnen
        float grayScaleFloat = 0.299f * red + 0.587f * green + 0.114f * blue;
        uchar grayScale = (uchar)grayScaleFloat;

        // Den berechneten Graustufenwert für die Farbkanäle schreiben
        buffer[pixelOffset] = grayScale;     // Rot
        buffer[pixelOffset + 1] = grayScale; // Grün
        buffer[pixelOffset + 2] = grayScale; // Blau
        // Alpha-Kanal unverändert lassen
        buffer[pixelOffset + 3] = alpha;

    }
}