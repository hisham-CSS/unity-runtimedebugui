# Changelog

All notable changes to this package will be documented in this file.

## [1.1.0] - 2025-06-10

### Added
- **Mobile Support System** - Comprehensive mobile device support with multiple trigger options
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

### Changed
- **UI Initialization** - Debug panel container is now properly captured during initialization
- **Toggle Visibility Logic** - Now correctly hides only the debug panel, keeping mobile button accessible
- **Mobile Button Behavior** - Toggle button now properly shows when panel is hidden and hides when panel is visible
- **Touch Input Handling** - Optimized touch detection with minimal performance impact

### Fixed
- **Mobile Button Visibility** - Fixed issue where mobile toggle button would disappear when debug panel was hidden
- **Touch Input Conflicts** - Prevented mobile gestures from interfering with normal gameplay input
- **Cross-Platform Compatibility** - Ensured mobile features don't break desktop functionality

### Technical Details
- Added `MobileTriggerType` enum for trigger method selection
- Implemented efficient touch tracking with configurable gesture recognition
- Enhanced CSS with mobile-specific responsive breakpoints
- Added proper touch event handling for iOS, Android, and WebGL platforms



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