/* eslint-disable jsx-a11y/alt-text */
import { Box, Button, IconButton, Typography } from "@mui/material";


const BASE_URL = "https://otbemekhfiye12.bsite.net";
const DEFAULT_IMAGE = "/default-image.png"; // Update this if needed

const ImageUploader = ({
  currentItem,
  setCurrentItem,
  imagesFiles,
  setImagesFiles,
}) => {
  const handleAddImage = (e) => {
    const files = Array.from(e.target.files || []);
    if (files.length === 0) return;

    const newImageEntries = files.map((file) => ({
      id: `__new__${Date.now()}${Math.random()}`, // unique placeholder
      file,
    }));

    setImagesFiles((prev) => [
      ...prev,
      ...newImageEntries.map((entry) => entry.file),
    ]);
    setCurrentItem((prev) => ({
      ...prev,
      images: [...prev.images, ...newImageEntries.map((entry) => entry.id)],
    }));
  };

  const getFileForIndex = (imgId) => {
    const newIndexes = currentItem.images
      .map((img, i) => (img.startsWith("__new__") ? i : null))
      .filter((i) => i !== null);

    const index = newIndexes.indexOf(currentItem.images.indexOf(imgId));
    return imagesFiles[index];
  };

  const handleDelete = (index) => {
    const updatedImages = [...currentItem.images];
    const removedImage = updatedImages.splice(index, 1)[0];

    let updatedFiles = [...imagesFiles];
    if (removedImage.startsWith("__new__")) {
      const newIndexes = currentItem.images
        .map((img, i) => (img.startsWith("__new__") ? i : null))
        .filter((i) => i !== null);
      const removeIndex = newIndexes.indexOf(index);
      updatedFiles.splice(removeIndex, 1);
    }

    setImagesFiles(updatedFiles);
    setCurrentItem({ ...currentItem, images: updatedImages });
  };

  const handleReplace = (index, file) => {
    const updatedFiles = [...imagesFiles, file];
    const updatedImages = [...currentItem.images];
    updatedImages[index] = `__new__${Date.now()}${Math.random()}`;

    setImagesFiles(updatedFiles);
    setCurrentItem({ ...currentItem, images: updatedImages });
  };

  return (
    <Box mt={4}>
      <Typography
        variant="subtitle1"
        sx={{ color: "#00d4ff", fontWeight: "bold", mb: 2 }}
      >
        Images
      </Typography>

      <Box display="flex" flexWrap="wrap" gap={2}>
        {/* + Add Image Button */}
        <Box>
          <Button
            variant="outlined"
            component="label"
            sx={{
              width: 100,
              height: 100,
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              borderRadius: 2,
              borderColor: "#00d4ff",
              color: "#00d4ff",
              textTransform: "none",
              fontSize: 12,
              backgroundColor: "transparent",
              "&:hover": {
                backgroundColor: "rgba(0, 212, 255, 0.1)",
              },
            }}
          >
            + Add
            <input
              type="file"
              hidden
              accept="image/*"
              multiple
              onChange={handleAddImage}
            />
          </Button>
        </Box>

        {/* Render Images */}
        {currentItem.images.map((img, index) => {
          const isNew = img.startsWith("__new__");
          const file = isNew ? getFileForIndex(img) : null;
          const imageUrl = isNew
            ? file && URL.createObjectURL(file)
            : img.startsWith("http") || img.startsWith("data:")
            ? img
            : `${BASE_URL}${img}`;

          return (
            <Box key={index} position="relative">
              <Box
                sx={{
                  width: 100,
                  height: 100,
                  borderRadius: 2,
                  border: "2px solid #2a2a35",
                  overflow: "hidden",
                  position: "relative",
                  backgroundColor: "#f0f0f0",
                }}
              >
                <img
                  src={imageUrl}
                  style={{ width: "100%", height: "100%", objectFit: "cover" }}
                  onError={(e) => {
                    e.target.onerror = null;
                    e.target.src = DEFAULT_IMAGE;
                  }}
                />
                {isNew && (
                  <Box
                    position="absolute"
                    bottom={0}
                    width="100%"
                    bgcolor="rgba(0,0,0,0.6)"
                    color="white"
                    fontSize={10}
                    textAlign="center"
                    py={0.5}
                  >
                    Image added
                  </Box>
                )}
              </Box>

              {/* Delete Image */}
              <IconButton
                size="small"
                onClick={() => handleDelete(index)}
                sx={{
                  position: "absolute",
                  top: 4,
                  right: 4,
                  backgroundColor: "rgba(239, 68, 68, 0.9)",
                  color: "white",
                  width: 24,
                  height: 24,
                  fontSize: 16,
                  "&:hover": {
                    backgroundColor: "red",
                  },
                }}
              >
                ✕
              </IconButton>

              {/* Replace Image */}
              {!isNew && (
                <Button
                  size="small"
                  component="label"
                  variant="outlined"
                  fullWidth
                  sx={{
                    mt: 1,
                    color: "#00d4ff",
                    borderColor: "#00d4ff",
                    fontSize: "10px",
                    textTransform: "none",
                  }}
                >
                  Replace
                  <input
                    hidden
                    type="file"
                    accept="image/*"
                    onChange={(e) => {
                      if (e.target.files?.length > 0) {
                        handleReplace(index, e.target.files[0]);
                      }
                    }}
                  />
                </Button>
              )}
            </Box>
          );
        })}
      </Box>
    </Box>
  );
};

export default ImageUploader;
