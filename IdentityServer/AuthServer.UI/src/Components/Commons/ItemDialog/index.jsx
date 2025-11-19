import React from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Grid,
  MenuItem,
  Typography,
  CircularProgress,
} from "@mui/material";
import ColorPicker from "./../ColorPicker";
import SizeManager from "./../SizeManager";
import ImageManager from "./../ImageUploader";

const ItemDialog = ({
  openDialog,
  currentItem,
  setCurrentItem,
  handleCloseDialog,
  handleSubmit,
  categories,
  categoriesLoading,
  loading,
  selectedColor,
  setSelectedColor,
  newColors,
  setNewColors,
  newSizes,
  setNewSizes,
  imagesFiles,
  setImagesFiles,
}) => {
  return (
    <Dialog
      open={openDialog}
      onClose={handleCloseDialog}
      maxWidth="md"
      fullWidth
      sx={{
        "& .MuiDialog-paper": {
          backgroundColor: "#FFFFF",
          color: "#FFFFF",
          border: "1px solid #2a2a35",
        },
      }}
    >
      <DialogTitle
        sx={{
          color: "#00d4ff",
          fontWeight: "bold",
          backgroundColor: "#1a1a1f",
          borderBottom: "1px solid #2a2a35",
        }}
      >
        {currentItem.id > 0 ? "Edit Item" : "Add Item"}
      </DialogTitle>
      <DialogContent sx={{ backgroundColor: "#1a1a1f", pt: 2 }}>
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}>
            <TextField
              label="Name"
              fullWidth
              margin="normal"
              required
              value={currentItem.name}
              onChange={(e) =>
                setCurrentItem({ ...currentItem, name: e.target.value })
              }
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
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField
              label="Price"
              type="number"
              fullWidth
              margin="normal"
              required
              inputProps={{ min: 0, step: 0.01 }}
              value={currentItem.price}
              onFocus={(e) => e.target.select()}
              onChange={(e) =>
                setCurrentItem({
                  ...currentItem,
                  price: parseFloat(e.target.value) || 0,
                })
              }
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
          </Grid>
          <Grid item xs={12}>
            <TextField
              label="Description"
              fullWidth
              margin="normal"
              multiline
              rows={3}
              value={currentItem.description}
              onChange={(e) =>
                setCurrentItem({
                  ...currentItem,
                  description: e.target.value,
                })
              }
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
          </Grid>
          <Grid item xs={12}>
            <TextField
              label="Category"
              select
              fullWidth
              margin="normal"
              required
              value={currentItem.categoryId}
              onChange={(e) =>
                setCurrentItem({ ...currentItem, categoryId: e.target.value })
              }
              disabled={categoriesLoading || categories.length === 0}
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
            >
              {categories.map((cat) => (
                <MenuItem key={cat.id} value={cat.id}>
                  {cat.name}
                </MenuItem>
              ))}
            </TextField>
            {categoriesLoading && (
              <Typography variant="caption" sx={{ color: "#b0b0b5", ml: 1 }}>
                Loading categories...
              </Typography>
            )}
          </Grid>
        </Grid>

        <ColorPicker
          selectedColor={selectedColor}
          setSelectedColor={setSelectedColor}
          colors={newColors}
          setColors={setNewColors}
        />

        <SizeManager sizes={newSizes} setSizes={setNewSizes} />

        <ImageManager
          currentItem={currentItem}
          setCurrentItem={setCurrentItem}
          imagesFiles={imagesFiles}
          setImagesFiles={setImagesFiles}
        />
      </DialogContent>
      <DialogActions
        sx={{
          backgroundColor: "#1a1a1f",
          borderTop: "1px solid #2a2a35",
          p: 3,
        }}
      >
        <Button
          onClick={handleCloseDialog}
          variant="outlined"
          sx={{
            color: "#ef4444",
            borderColor: "#ef4444",
            backgroundColor: "transparent",
            "&:hover": {
              backgroundColor: "rgba(239, 68, 68, 0.1)",
              borderColor: "#f87171",
            },
          }}
        >
          Cancel
        </Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={
            loading || !currentItem.name.trim() || !currentItem.categoryId
          }
          sx={{
            backgroundColor: "#00d4ff",
            color: "#0a0a0b",
            border: "none",
            fontWeight: "bold",
            "&:hover": {
              backgroundColor: "#0dd9ff",
            },
            "&:disabled": {
              backgroundColor: "#666666",
              color: "#999999",
            },
          }}
        >
          {loading ? (
            <CircularProgress size={20} sx={{ color: "#999999" }} />
          ) : (
            "Save"
          )}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
export default ItemDialog;