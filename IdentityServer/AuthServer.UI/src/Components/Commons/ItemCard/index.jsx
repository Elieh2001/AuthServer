import React from "react";
import {
  Card,
  CardContent,
  CardMedia,
  Typography,
  Button,
  Box,
  IconButton,
} from "@mui/material";
import ArrowBackIosIcon from "@mui/icons-material/ArrowBackIos";
import ArrowForwardIosIcon from "@mui/icons-material/ArrowForwardIos";

const BASE_URL = "https://otbemekhfiye12.bsite.net";
const DEFAULT_IMAGE = "";

const ItemCard = ({ 
  item, 
  imageIndex, 
  onEdit, 
  onDelete, 
  onNextImage, 
  onPrevImage, 
  getCategoryName 
}) => {
  const images =
    item.thumbnail && item.thumbnail.length > 0
      ? item.thumbnail
      : item.images && item.images.length > 0
      ? item.images
      : [DEFAULT_IMAGE];

  const renderColors = () => {
    let colors = [];
    if (Array.isArray(item.colors)) {
      colors = item.colors.flatMap((colorGroup) =>
        typeof colorGroup === "string"
          ? colorGroup.split(",")
          : [colorGroup]
      );
    } else if (item.colors) {
      colors = item.colors.split(",");
    }

    return colors.length > 0 ? (
      colors.map((color, index) => (
        <Box
          key={index}
          sx={{
            width: 15,
            height: 15,
            backgroundColor: color.trim(),
            display: "inline-block",
            ml: 1,
            borderRadius: "50%",
            border: "2px solid #000000",
          }}
          title={color.trim()}
        />
      ))
    ) : (
      <span style={{ marginLeft: 8, color: "#666666" }}>None</span>
    );
  };

  const renderSizes = () => {
    let sizes = [];
    if (Array.isArray(item.sizes)) {
      sizes = item.sizes.flatMap((sizeGroup) =>
        typeof sizeGroup === "string"
          ? sizeGroup.split(",")
          : [sizeGroup]
      );
    } else if (item.sizes) {
      sizes = item.sizes.split(",");
    }
    return sizes.length > 0 ? sizes.join(", ") : "None";
  };

  return (
    <Card
      sx={{
        position: "relative",
        borderRadius: 3,
        boxShadow: "0 8px 25px rgba(0, 0, 0, 0.1)",
        backgroundColor: "white",
        border: "2px solid #000000",
        transition: "all 0.3s ease",
        "&:hover": {
          boxShadow: "0 12px 35px rgba(0, 0, 0, 0.2)",
          transform: "translateY(-5px)",
          border: "2px solid #000000",
        },
      }}
    >
      <CardMedia
        component="img"
        height="200"
        image={`${BASE_URL}${images[imageIndex]}`}
        alt={item.name}
        onError={(e) => {
          e.target.onerror = null;
          e.target.src = DEFAULT_IMAGE;
        }}
      />
      {images.length > 1 && (
        <>
          <Box sx={{ position: "absolute", top: "45%", left: 10 }}>
            <IconButton
              onClick={() => onPrevImage(item.id, images.length)}
              sx={{
                backgroundColor: "rgba(0, 0, 0, 0.7)",
                color: "white",
                "&:hover": {
                  backgroundColor: "rgba(0, 0, 0, 0.9)",
                },
              }}
              size="small"
            >
              <ArrowBackIosIcon fontSize="small" />
            </IconButton>
          </Box>
          <Box sx={{ position: "absolute", top: "45%", right: 10 }}>
            <IconButton
              onClick={() => onNextImage(item.id, images.length)}
              sx={{
                backgroundColor: "rgba(0, 0, 0, 0.7)",
                color: "white",
                "&:hover": {
                  backgroundColor: "rgba(0, 0, 0, 0.9)",
                },
              }}
              size="small"
            >
              <ArrowForwardIosIcon fontSize="small" />
            </IconButton>
          </Box>
        </>
      )}
      <CardContent sx={{ backgroundColor: "white" }}>
        <Typography
          variant="h6"
          fontWeight="bold"
          sx={{ color: "#000000" }}
        >
          {item.name}
        </Typography>
        <Typography
          variant="body2"
          sx={{ color: "#666666", mb: 1 }}
        >
          {item.description}
        </Typography>
        <Typography
          variant="body2"
          sx={{ color: "#000000", fontWeight: "bold", mb: 1 }}
        >
          Price: ${item.price}
        </Typography>
        <Typography
          variant="body2"
          sx={{ color: "#666666", mb: 1 }}
        >
          Category: {getCategoryName(item.categoryId)}
        </Typography>
        <Typography
          variant="body2"
          sx={{
            color: "#000000",
            mt: 1,
            display: "flex",
            alignItems: "center",
          }}
        >
          Colors:
          {renderColors()}
        </Typography>
        <Typography variant="body2" sx={{ color: "#000000" }}>
          Sizes: {renderSizes()}
        </Typography>
        <Box display="flex" justifyContent="space-between" mt={2}>
          <Button
            size="small"
            variant="contained"
            onClick={() => onEdit(item)}
            sx={{
              borderRadius: 2,
              backgroundColor: "#000000",
              color: "white",
              border: "2px solid #000000",
              "&:hover": {
                backgroundColor: "#333333",
              },
            }}
          >
            Edit
          </Button>
          <Button
            size="small"
            variant="outlined"
            onClick={() => onDelete(item.id)}
            sx={{
              borderRadius: 2,
              color: "#dc3545",
              borderColor: "#dc3545",
              backgroundColor: "transparent",
              "&:hover": {
                backgroundColor: "rgba(220, 53, 69, 0.1)",
                borderColor: "#dc3545",
              },
            }}
          >
            Delete
          </Button>
        </Box>
      </CardContent>
    </Card>
  );
};
export default ItemCard;