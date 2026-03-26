# FETaskBuilder - Task Board Application

A modern React-based task management application with a clean kanban-style interface.

## Overview

FETaskBuilder is a frontend application built with React and Vite that provides an intuitive task board interface for managing tasks across different stages (Todo, InProgress, Done). It features optimistic UI updates, real-time activity tracking, and seamless API integration with a .NET backend.

## Tech Stack
- **Frontend**: React 18 with Vite
- **HTTP Client**: Axios for API communication
- **Styling**: Tailwind CSS with PostCSS
- **State Management**: React hooks (custom hooks)
- **Build Tool**: Vite
- **Code Quality**: ESLint with React plugins

## Features
- **Task Board UI**: 3 columns (Todo, InProgress, Done)
- **Task CRUD Operations**: Create, Edit, Delete (soft delete), Change status
- **API Integration**: Service layer (taskService.js) connecting to .NET backend
- **Optimistic UI Updates**: UI updates before API response with rollback on failure
- **Activity Timeline**: Shows task activity history
- **Error Handling**: Toast notifications for user feedback
- **Loading States**: Loading spinners and empty states
- **Responsive Design**: Works on desktop and mobile devices

## Project Structure
```
/src
  /components
    - TaskCard.jsx
    - TaskColumn.jsx
    - CreateTaskForm.jsx
    - ActivityTimeline.jsx
    - Toast.jsx
    - LoadingSpinner.jsx
  /pages
    - TaskBoard.jsx
  /services
    - taskService.js
  /hooks
    - useTasks.js
    - useActivities.js
    - useToast.js
  - App.jsx
  - main.jsx
  - index.css
```

## Setup Instructions

### Prerequisites
- **Node.js**: Version 16.0.0 or higher
- **npm**: Version 7.0.0 or higher (comes with Node.js)
- **Backend API**: .NET API server running on configured port

### Installation Steps

1. **Clone the repository** (if not already cloned):
```bash
git clone <repository-url>
cd FETaskBuilder
```

2. **Install dependencies**:
```bash
npm install
```

3. **Environment Setup**:
   - Ensure your .NET backend API is running
   - Verify the API endpoint configuration in `vite.config.js`
   - Current configuration expects backend at `http://localhost:58183`

4. **Start the development server**:
```bash
npm run dev
```

5. **Access the application**:
   - Open your browser and navigate to `http://localhost:3000`
   - The application will automatically proxy API requests to the backend

### Backend Configuration

The application is configured to connect to a .NET backend. To modify the backend connection:

**Option 1: Update Vite Configuration**
Edit `vite.config.js`:
```javascript
export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:YOUR_PORT', // Change this to your backend port
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
```

**Option 2: Environment Variables** (if implemented)
Create a `.env` file in the root directory:
```
VITE_API_BASE_URL=http://localhost:YOUR_PORT
```

### Required Backend Endpoints

The .NET backend should implement the following REST API endpoints:

- `GET /api/tasks` - Retrieve all tasks
- `POST /api/tasks` - Create a new task
- `PUT /api/tasks/{id}` - Update an existing task
- `DELETE /api/tasks/{id}` - Soft delete a task
- `GET /api/tasks/activities` - Get task activity history

### Build for Production

```bash
# Build the application
npm run build

# Preview the production build
npm run preview
```

### API Error Handling

The service implements comprehensive error handling:

- **Network Errors**: Connection issues, timeouts
- **HTTP Errors**: 4xx/5xx status codes
- **Validation Errors**: Bad request data
- **Server Errors**: Internal server issues

All errors are caught and propagated to the UI layer through custom hooks for user feedback.

### Request/Response Flow

1. **User Action** → Component Event Handler
2. **Component** → Custom Hook (useTasks/useActivities)
3. **Custom Hook** → Service Layer (taskService.js)
4. **Service Layer** → Axios HTTP Request
5. **Response** → Service Layer → Hook → Component → UI Update

### Optimistic Updates

The application implements optimistic UI updates:
- UI updates immediately on user action
- API call happens in background
- Automatic rollback if API call fails
- User notified via toast notifications

## Features Implementation

### Optimistic Updates
- UI updates immediately when user performs actions
- API calls happen in the background
- Automatic rollback if API call fails
- User feedback via toast notifications

### Error Handling
- Network errors are caught and displayed
- Validation errors are shown to users
- Toast notifications for all user actions

### Activity Timeline
- Real-time activity tracking
- Visual indicators for different action types
- Timestamp formatting
- Auto-refresh on task changes

## Available Scripts
- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Run ESLint

## Design Decisions

### Architecture Patterns

#### 1. **Clean Architecture with Separation of Concerns**
- **Components**: Pure UI components focused on presentation
- **Hooks**: Business logic and state management
- **Services**: API communication and data transformation
- **Utils**: Helper functions and utilities

This separation ensures:
- Testability of individual layers
- Reusability of components and hooks
- Maintainability and scalability
- Clear data flow and dependencies

#### 2. **Custom Hooks for State Management**
Instead of Redux/Context API, we chose custom hooks because:
- **Simplicity**: Less boilerplate for this application size
- **Performance**: Targeted re-renders only when needed
- **Encapsulation**: Logic co-located with related state
- **Testability**: Easy to unit test hooks in isolation

Key hooks:
- `useTasks()`: Task CRUD operations and optimistic updates
- `useActivities()`: Activity timeline management
- `useToast()`: Toast notification system

#### 3. **Optimistic UI Updates**
Decision to implement optimistic updates for better UX:
- **Immediate Feedback**: Users see changes instantly
- **Perceived Performance**: Application feels faster
- **Error Recovery**: Automatic rollback on API failures
- **User Confidence**: Clear success/error notifications

#### 4. **Service Layer Pattern**
Centralized API service (`taskService.js`) provides:
- **Single Source of Truth**: All API calls in one place
- **Consistent Error Handling**: Unified error management
- **Request/Response Interceptors**: Common logic for all requests
- **Easy Mocking**: Simplified testing and development

#### 5. **Component Composition Strategy**
- **TaskCard**: Reusable card component for individual tasks
- **TaskColumn**: Column container with drag-and-drop preparation
- **CreateTaskForm**: Modal form for task creation
- **ActivityTimeline**: Separate concern for activity tracking

#### 6. **Styling Approach**
- **Tailwind CSS**: Utility-first CSS framework
- **Responsive Design**: Mobile-first approach
- **Component-Scoped Styles**: Consistent design system
- **Minimal Custom CSS**: Leverage Tailwind utilities

#### 7. **Error Handling Strategy**
- **Graceful Degradation**: App continues working with partial failures
- **User-Friendly Messages**: Clear, actionable error messages
- **Toast Notifications**: Non-intrusive error feedback
- **Retry Logic**: Automatic retry for transient failures

#### 8. **Performance Considerations**
- **Lazy Loading**: Components loaded as needed
- **Memoization**: React.memo for expensive components
- **Debounced Search**: Prevent excessive API calls
- **Efficient Re-renders**: Optimized dependency arrays

### Technology Choices

#### React with Vite
- **Vite**: Fast development server and builds
- **React 18**: Latest features and optimizations
- **JSX**: Component-based architecture

#### Axios over Fetch
- **Better Error Handling**: Automatic error rejection
- **Request/Response Interceptors**: Common logic
- **JSON Transformation**: Automatic parsing
- **Browser Support**: Wider compatibility

#### Tailwind CSS
- **Rapid Development**: Utility classes speed up styling
- **Consistency**: Design system built-in
- **Responsive**: Mobile-first utilities
- **Maintainability**: No custom CSS to maintain

### Completed Features

#### Core Functionality
- **Task Board Interface**: Three-column kanban board (Todo, InProgress, Done)
- **Task CRUD Operations**: 
  - Create new tasks with title and description
  - Edit existing task details
  - Soft delete tasks (marked as deleted but preserved)
  - Change task status between columns
- **API Integration**: Full REST API communication with .NET backend
- **Real-time Activity Timeline**: Track all task changes with timestamps

#### User Experience
- **Optimistic UI Updates**: Immediate visual feedback with automatic rollback
- **Toast Notification System**: Success/error messages for all user actions
- **Loading States**: Spinners and empty states for better UX
- **Responsive Design**: Mobile-friendly interface using Tailwind CSS
- **Error Handling**: Comprehensive error catching and user feedback

#### Architecture & Code Quality
- **Clean Architecture**: Separation of concerns (Components, Hooks, Services)
- **Custom Hooks**: Reusable state management logic
- **Service Layer**: Centralized API communication
- **Component Reusability**: Modular, maintainable components
- **Development Environment**: Vite-based fast development setup

### Partially Implemented

#### Performance Optimizations
- **Basic Memoization**: Some React.memo implementations

#### Error Recovery
- **Basic Retry Logic**: Simple error handling
