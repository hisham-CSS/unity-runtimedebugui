# Changelog

All notable changes to this package will be documented in this file.

## [1.3.0] - 2025-06-15

### Added
- **Enhanced Auto-Save System** 
  - Intelligent saving with performance optimization
  - Four auto-save modes: Immediate, Debounced (default), Interval, and Manual
  - Debounced mode reduces disk writes by 99% during active slider use
  - Configurable save delays and intervals for optimal performance
  - App event saving (pause, focus loss, component destruction)
  - Visual save status indicators with real-time feedback
- **Save Status Indicator** 
  - Real-time visual feedback system
  - "Unsaved Changes" indicator (orange) when modifications are pending
  - "Saving..." indicator (yellow) during save operations
  - "Saved" confirmation (green) with auto-hide after 2 seconds
  - Positioned in bottom-right corner with smooth transitions
- **Enhanced Save Button** 
  - Improved manual save functionality
  - Visual state changes: normal → unsaved (orange) → saving confirmation
  - "Save Settings *" text indicates pending changes
  - "Saved!" confirmation with temporary disable and color change
  - Proper button state management without closure issues
- **Performance Optimizations** 
  - Significant improvements for production use
  - Smart change tracking prevents unnecessary save operations
  - Debounced saving eliminates disk I/O during slider dragging
  - Configurable auto-save behavior for different use cases
  - SSD-friendly operation with minimal write cycles

## [1.2.0] - 2025-06-11

### Added
- **Custom Editor System** 
  - Professional inspector interface with enhanced UX
  - Conditional field visibility based on feature toggles
  - Organized foldout sections for better navigation
  - Real-time help text and contextual guidance
  - Smart field validation with automatic range clamping
  - Save location preview for file-based serialization
- **Enhanced Inspector Organization**
  - UI Configuration section with core setup options
  - Mobile Support section (only visible when enabled)
  - Serialization section (only visible when enabled)
  - Tooltip System section with hover configuration
- **Smart Field Management**
  - Mobile options only appear when `enableMobileSupport = true`
  - Serialization options only appear when `enableSerialization = true`
  - Context-sensitive fields based on trigger type selection
  - PlayerPrefs vs File options toggle relevant sub-fields

## [1.1.0] - 2025-06-10

### Added
- **Mobile Support System** 
  - Comprehensive mobile device support with multiple trigger options
  - Multi-touch gesture trigger (3-finger tap by default, configurable)
  - Touch-and-hold trigger (2-second hold by default, configurable)
  - On-screen toggle button option for maximum accessibility
  - Smart mobile platform detection
  - Mobile-responsive UI adjustments
- **Mobile Configuration Options**
  - `enableMobileSupport` - Toggle mobile input handling
  - `mobileTriggerType` - Choose between TouchGesture, TouchAndHold, or OnScreenButton
  - `touchCount` - Configure number of fingers for multi-touch gesture
  - `touchHoldTime` - Configure hold duration for touch-and-hold trigger
  - `showToggleButton` - Option to show/hide on-screen toggle button
  - `toggleButtonText` - Customizable button text
- **Enhanced Platform Support**
  - iOS and Android native touch input support
  - WebGL mobile browser compatibility
  - Desktop platforms retain keyboard functionality while gaining mobile triggers
- **Improved UI Architecture**
  - Better separation of debug panel container from root element
  - Fixed mobile button visibility logic
  - Enhanced responsive design for touch devices
  
## [1.0.0] - 2025-06-06

### Added
- Initial release of RuntimeDebugUI System
- Real-time parameter tweaking with sliders, toggles, and info displays
- Smart auto-save system with accessible file locations
- Cross-platform compatibility
- Runtime tooltip system
- Organized tab system with automatic scrolling
- Data-driven configuration approach
- Clean JSON serialization with configurable decimal precision

### Features
- Slider controls with customizable ranges
- Toggle controls for boolean values
- Info display controls for read-only data
- Section organization within tabs
- Auto-save functionality with visual indicators
- Smart file location selection (accessible locations with fallback)
- Custom tooltip system that works in builds
- Responsive design with mobile support