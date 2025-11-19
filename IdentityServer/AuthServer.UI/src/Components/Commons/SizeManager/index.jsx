import React from "react";
import { Box, TextField, Chip, Typography } from "@mui/material";

const SizeManager = ({ sizes, setSizes }) => {
  const handleSizeAdd = (e) => {
    if (e.key === "Enter" && e.target.value.trim()) {
      const newSize = e.target.value.trim();
      if (!sizes.includes(newSize)) {
        setSizes([...sizes, newSize]);
      }
      e.target.value = "";
    }
  };

  const handleDeleteSize = (indexToDelete) => {
    setSizes(sizes.filter((_, index) => index !== indexToDelete));
  };

  return (
    <Box>
      <Typography
        variant="subtitle1"
        mt={3}
        sx={{ color: "#00d4ff", fontWeight: "bold" }}
      >
        Sizes
      </Typography>
      <Box display="flex" gap={1} mt={1} mb={2}>
        <TextField
          label="Add Size (Press Enter)"
          size="small"
          fullWidth
          onKeyDown={handleSizeAdd}
          placeholder="e.g., S, M, L, XL"
          sx={{
            "& .MuiInputLabel-root": { color: "#b0b0b5" },
            "& .MuiInputLabel-root.Mui-focused": { color: "#00d4ff" },
            "& .MuiOutlinedInput-root": {
              color: "#e0e0e0",
              backgroundColor: "#0a0a0b",
              "& fieldset": { borderColor: "#2a2a35" },
              "&:hover fieldset": { borderColor: "#00d4ff" },
              "&.Mui-focused fieldset": { borderColor: "#00d4ff" },
            },
          }}
        />
      </Box>
      <Box>
        {sizes.map((size, index) => (
          <Chip
            key={index}
            label={size}
            onDelete={() => handleDeleteSize(index)}
            sx={{
              mr: 1,
              mb: 1,
              backgroundColor: "#7c3aed",
              color: "#e0e0e0",
              border: "1px solid #00d4ff",
              "& .MuiChip-deleteIcon": {
                color: "#e0e0e0",
              },
            }}
          />
        ))}
      </Box>
    </Box>
  );
};
export default SizeManager;