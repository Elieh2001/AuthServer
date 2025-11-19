import React, { useState } from "react";
import { ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css"; // required for toast styling

// Shared size classes (used in multiple components)
const sizeClasses = {
  small: "w-4 h-4",
  medium: "w-8 h-8",
  large: "w-12 h-12",
  xl: "w-16 h-16"
};

const LoadingSpinner = ({
  text = "Loading...",
  size = "medium",
  color = "primary",
  fullScreen = false,
  overlay = false
}) => {
  const colorClasses = {
    primary: "border-blue-600",
    secondary: "border-gray-600",
    success: "border-green-600",
    warning: "border-yellow-600",
    error: "border-red-600",
    white: "border-white",
    black: "border-black"
  };

  const textSizeClasses = {
    small: "text-sm",
    medium: "text-base",
    large: "text-lg",
    xl: "text-xl"
  };

  const spinnerClasses = `
    ${sizeClasses[size]} 
    border-4 border-gray-200 border-t-transparent 
    ${colorClasses[color]}
    rounded-full animate-spin
  `;

  const containerClasses = fullScreen
    ? "fixed inset-0 flex flex-col items-center justify-center bg-white z-50"
    : "flex flex-col items-center justify-center p-4";

  const overlayClasses = overlay
    ? "fixed inset-0 flex flex-col items-center justify-center bg-black bg-opacity-50 z-50"
    : containerClasses;

  return (
    <div className={overlay ? overlayClasses : containerClasses}>
      <div className={spinnerClasses}></div>
      {text && (
        <p className={`mt-3 text-gray-600 font-medium ${textSizeClasses[size]}`}>
          {text}
        </p>
      )}
    </div>
  );
};

// Spinner variants
export const CenteredSpinner = ({ text, size = "large" }) => (
  <div className="flex flex-col items-center justify-center min-h-64">
    <LoadingSpinner text={text} size={size} />
  </div>
);

export const InlineSpinner = ({ text, size = "small" }) => (
  <div className="flex items-center space-x-2">
    <LoadingSpinner text="" size={size} />
    {text && <span className="text-sm text-gray-600">{text}</span>}
  </div>
);

export const OverlaySpinner = ({ text, size = "large" }) => (
  <LoadingSpinner text={text} size={size} overlay={true} />
);

export const FullScreenSpinner = ({ text, size = "xl" }) => (
  <LoadingSpinner text={text} size={size} fullScreen={true} />
);

export const ButtonSpinner = ({ size = "small", className = "" }) => (
  <div className={`inline-block ${sizeClasses[size]} border-2 border-gray-300 border-t-transparent rounded-full animate-spin ${className}`}></div>
);

// Demo
const LoadingSpinnerDemo = () => {
  const [showOverlay, setShowOverlay] = useState(false);
  const [showFullScreen, setShowFullScreen] = useState(false);

  return (
    <div className="p-8 space-y-8 bg-gray-50 min-h-screen">
      <h1 className="text-3xl font-bold text-gray-800 mb-8">Loading Spinner Components</h1>

      {/* Basic Sizes */}
      <section className="bg-white p-6 rounded-lg shadow-md">
        <h2 className="text-xl font-semibold mb-4">Basic Spinners</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {["small", "medium", "large", "xl"].map(size => (
            <div className="text-center" key={size}>
              <h3 className="text-sm font-medium mb-2 capitalize">{size}</h3>
              <LoadingSpinner size={size} text="Loading..." />
            </div>
          ))}
        </div>
      </section>

      {/* Color Variants */}
      <section className="bg-white p-6 rounded-lg shadow-md">
        <h2 className="text-xl font-semibold mb-4">Color Variants</h2>
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
          {["primary", "secondary", "success", "warning", "error", "black"].map(color => (
            <div key={color} className="text-center">
              <h3 className="text-xs font-medium mb-2 capitalize">{color}</h3>
              <LoadingSpinner size="medium" color={color} text="" />
            </div>
          ))}
        </div>
      </section>

      {/* Spinner Types */}
      <section className="bg-white p-6 rounded-lg shadow-md">
        <h2 className="text-xl font-semibold mb-4">Specialized Variants</h2>
        <div className="space-y-6">
          <div>
            <h3 className="text-sm font-medium mb-2">Centered Spinner</h3>
            <div className="border border-gray-200 rounded">
              <CenteredSpinner text="Loading data..." />
            </div>
          </div>

          <div>
            <h3 className="text-sm font-medium mb-2">Inline Spinner</h3>
            <div className="flex items-center space-x-4">
              <InlineSpinner text="Saving changes..." />
              <InlineSpinner text="Fetching data..." />
              <InlineSpinner text="Processing..." />
            </div>
          </div>

          <div>
            <h3 className="text-sm font-medium mb-2">Button with Spinner</h3>
            <div className="flex space-x-4">
              <button className="flex items-center space-x-2 bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 disabled:opacity-50" disabled>
                <ButtonSpinner />
                <span>Loading...</span>
              </button>
              <button className="flex items-center space-x-2 bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700 disabled:opacity-50" disabled>
                <ButtonSpinner />
                <span>Saving...</span>
              </button>
            </div>
          </div>
        </div>
      </section>

      {/* Interactive Buttons */}
      <section className="bg-white p-6 rounded-lg shadow-md">
        <h2 className="text-xl font-semibold mb-4">Interactive Demos</h2>
        <div className="space-x-4">
          <button
            onClick={() => setShowOverlay(true)}
            className="bg-purple-600 text-white px-4 py-2 rounded hover:bg-purple-700"
          >
            Show Overlay Spinner
          </button>
          <button
            onClick={() => setShowFullScreen(true)}
            className="bg-indigo-600 text-white px-4 py-2 rounded hover:bg-indigo-700"
          >
            Show Full Screen Spinner
          </button>
        </div>
      </section>

      {/* Code Samples */}
      <section className="bg-white p-6 rounded-lg shadow-md">
        <h2 className="text-xl font-semibold mb-4">Usage Examples</h2>
        <div className="bg-gray-100 p-4 rounded-lg">
          <pre className="text-sm overflow-x-auto">
{`<LoadingSpinner text="Loading..." size="medium" color="primary" />
<CenteredSpinner text="Loading data..." />
<InlineSpinner text="Saving..." />
<ButtonSpinner />
<OverlaySpinner text="Processing..." />
<FullScreenSpinner text="Please wait..." />`}
          </pre>
        </div>
      </section>

      {/* Overlays */}
      {showOverlay && (
        <div className="fixed inset-0 flex flex-col items-center justify-center bg-black bg-opacity-50 z-50">
          <LoadingSpinner text="Processing your request..." size="large" color="white" />
          <button
            onClick={() => setShowOverlay(false)}
            className="mt-4 bg-white text-black px-4 py-2 rounded hover:bg-gray-100"
          >
            Close
          </button>
        </div>
      )}

      {showFullScreen && (
        <div className="fixed inset-0 flex flex-col items-center justify-center bg-white z-50">
          <LoadingSpinner text="Loading application..." size="xl" color="primary" />
          <button
            onClick={() => setShowFullScreen(false)}
            className="mt-4 bg-gray-800 text-white px-4 py-2 rounded hover:bg-gray-700"
          >
            Close
          </button>
        </div>
      )}

      <ToastContainer position="bottom-right" />
    </div>
  );
};

export default LoadingSpinner;
export { LoadingSpinnerDemo };
