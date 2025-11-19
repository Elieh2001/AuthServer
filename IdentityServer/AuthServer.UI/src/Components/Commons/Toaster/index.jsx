import React, { useState, useEffect } from "react";
import { Snackbar, Alert as MuiAlert } from "@mui/material";

const Toaster = ({ type, message }) => {
  const [open, setOpen] = useState(true);

  useEffect(() => {
    setOpen(true); // Reopen the Snackbar whenever it's re-rendered
  }, [message]);

  const handleClose = (_, reason) => {
    if (reason === "clickaway") return; // Prevent closing on clickaway if needed
    setOpen(false);
  };

  return (
    <Snackbar
      open={open}
      autoHideDuration={3000} // 3 seconds
      onClose={handleClose}
      anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
    >
      <MuiAlert
        elevation={6}
        variant="filled"
        severity={type}
        onClose={handleClose}
        sx={{ width: "100%" }}
      >
        {message}
      </MuiAlert>
    </Snackbar>
  );
};

export default Toaster;
