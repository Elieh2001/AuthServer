import React from "react";
import { Box, Button, Chip, Typography } from "@mui/material";
import { SketchPicker } from "react-color";

const ColorPicker = ({ selectedColor, setSelectedColor, colors, setColors }) => {
  const handleAddColor = () => {
    if (selectedColor && !colors.includes(selectedColor)) {
      setColors([...colors, selectedColor]);
    }
  };

  const handleDeleteColor = (indexToDelete) => {
    setColors(colors.filter((_, index) => index !== indexToDelete));
  };

  return (
    <Box>
      <Typography
        variant="subtitle1"
        mt={3}
        sx={{ color: "#00d4ff", fontWeight: "bold" }}
      >
        Colors
      </Typography>
      <Box
        sx={{
          backgroundColor: "#0a0a0b",
          padding: 2,
          borderRadius: 2,
          border: "1px solid #2a2a35",
          mb: 2,
        }}
      >
        <SketchPicker
          color={selectedColor}
          onChange={(color) => setSelectedColor(color.hex)}
          width="40%"
          height="10%"
          disableAlpha={true}
        />
        <Button
          variant="contained"
          onClick={handleAddColor}
          sx={{
            mt: 2,
            backgroundColor: "#00d4ff",
            color: "#0a0a0b",
            "&:hover": {
              backgroundColor: "#0dd9ff",
            },
          }}
        >
          Add Color
        </Button>
      </Box>
      <Box>
        {colors.map((color, index) => (
          <Chip
            key={index}
            label={color}
            onDelete={() => handleDeleteColor(index)}
            sx={{
              backgroundColor: color,
              color:
                color === "#FFFFFF" || color === "#ffffff"
                  ? "#000000"
                  : "#ffffff",
              mr: 1,
              mb: 1,
              border: "1px solid #00d4ff",
              "& .MuiChip-deleteIcon": {
                color:
                  color === "#FFFFFF" || color === "#ffffff"
                    ? "#000000"
                    : "#ffffff",
              },
            }}
          />
        ))}
      </Box>
    </Box>
  );
};
export default ColorPicker;