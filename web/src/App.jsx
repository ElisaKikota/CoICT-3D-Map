import { useState, useEffect, useRef } from 'react'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import './App.css'
import Layout from './components/Layout.jsx'
import Home from './pages/Home.jsx'
import About from './pages/About.jsx'
import Academics from './pages/Academics.jsx'
import Research from './pages/Research.jsx'
import News from './pages/News.jsx'
import Events from './pages/Events.jsx'
import Contacts from './pages/Contacts.jsx'
import Announcements from './pages/Announcements.jsx'
import GenericNotFound from './pages/GenericNotFound.jsx'

function UnityTour() {
  const [showFullscreenPrompt, setShowFullscreenPrompt] = useState(false)
  const [isFullscreen, setIsFullscreen] = useState(false)
  const iframeRef = useRef(null)

  // Handle iframe load event
  useEffect(() => {
    const iframe = iframeRef.current
    if (!iframe) return

    const handleIframeLoad = () => {
      console.log('Unity game iframe loaded')
    }

    const handleIframeError = (error) => {
      console.error('Iframe error:', error)
      alert('Failed to load the game. Please check the browser console for details.')
    }

    iframe.addEventListener('load', handleIframeLoad)
    iframe.addEventListener('error', handleIframeError)
    
    return () => {
      if (iframe) {
        iframe.removeEventListener('load', handleIframeLoad)
        iframe.removeEventListener('error', handleIframeError)
      }
    }
  }, [])

  // Check resolution and show fullscreen prompt
  useEffect(() => {
    const checkResolution = () => {
      const width = window.innerWidth
      const height = window.innerHeight
      const targetWidth = 1080
      const targetHeight = 1920
      
      // Check if resolution matches (with some tolerance)
      const widthMatch = Math.abs(width - targetWidth) < 50
      const heightMatch = Math.abs(height - targetHeight) < 50
      
      // Also check if already in fullscreen
      const isCurrentlyFullscreen = 
        document.fullscreenElement !== null ||
        document.webkitFullscreenElement !== null ||
        document.mozFullScreenElement !== null ||
        document.msFullscreenElement !== null

      if (!widthMatch || !heightMatch) {
        if (!isCurrentlyFullscreen && !showFullscreenPrompt) {
          // Show prompt after a short delay to let game start
          setTimeout(() => {
            setShowFullscreenPrompt(true)
          }, 2000)
        }
      } else {
        setShowFullscreenPrompt(false)
      }
      
      setIsFullscreen(isCurrentlyFullscreen)
    }

    // Check after a delay to let the game initialize
    const timeoutId = setTimeout(checkResolution, 3000)
    
    // Check on resize and fullscreen change
    window.addEventListener('resize', checkResolution)
    document.addEventListener('fullscreenchange', checkResolution)
    document.addEventListener('webkitfullscreenchange', checkResolution)
    document.addEventListener('mozfullscreenchange', checkResolution)
    document.addEventListener('MSFullscreenChange', checkResolution)

    return () => {
      clearTimeout(timeoutId)
      window.removeEventListener('resize', checkResolution)
      document.removeEventListener('fullscreenchange', checkResolution)
      document.removeEventListener('webkitfullscreenchange', checkResolution)
      document.removeEventListener('mozfullscreenchange', checkResolution)
      document.removeEventListener('MSFullscreenChange', checkResolution)
    }
  }, [showFullscreenPrompt])

  const handleFullscreen = async () => {
    try {
      const element = document.documentElement
      
      if (element.requestFullscreen) {
        await element.requestFullscreen()
      } else if (element.webkitRequestFullscreen) {
        await element.webkitRequestFullscreen()
      } else if (element.mozRequestFullScreen) {
        await element.mozRequestFullScreen()
      } else if (element.msRequestFullscreen) {
        await element.msRequestFullscreen()
      } else {
        alert('Fullscreen is not supported by your browser. Please press F11 or use your browser\'s fullscreen button.')
        return
      }
      
      setShowFullscreenPrompt(false)
    } catch (error) {
      console.error('Error entering fullscreen:', error)
      alert('Could not enter fullscreen mode. Please press F11 or use your browser\'s fullscreen button.')
    }
  }

  const handleDismissPrompt = () => {
    setShowFullscreenPrompt(false)
  }

  return (
    <div className="fullscreen-page" style={{
      display: 'flex',
      flexDirection: 'column',
      height: '100vh',
      overflow: 'hidden',
      backgroundColor: '#242424',
      position: 'relative'
    }}>
      {showFullscreenPrompt && (
        <div className="fullscreen-prompt">
          <div className="fullscreen-prompt-content">
            <h3>Recommended: Fullscreen Mode</h3>
            <p>
              For the best experience, please use fullscreen mode.<br/>
              Optimal resolution: <strong>1080 x 1920</strong>
            </p>
            <div className="fullscreen-prompt-buttons">
              <button onClick={handleFullscreen} className="btn-fullscreen">
                Enter Fullscreen
              </button>
              <button onClick={handleDismissPrompt} className="btn-dismiss">
                Dismiss
              </button>
            </div>
          </div>
        </div>
      )}
      
      <iframe
        ref={iframeRef}
        src="/unity/index.html"
        style={{
          flex: 1,
          width: '100%',
          height: '100%',
          border: 'none',
          backgroundColor: '#000'
        }}
        allowFullScreen
        allow="autoplay; fullscreen"
        title="CoICT 3D Campus Map"
        onError={(e) => console.error('Iframe error:', e)}
        onLoad={() => console.log('Iframe loaded')}
      />
    </div>
  )
}

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/virtual-tours" element={<UnityTour />} />
        <Route path="/about/campus-virtual-tour" element={<Navigate to="/virtual-tours" replace />} />

        <Route element={<Layout />}>
          <Route path="/" element={<Home />} />

          <Route path="/about/:page" element={<About />} />
          <Route path="/academics/:category" element={<Academics />} />
          <Route path="/short-courses" element={<Navigate to="/academics/short-courses" replace />} />

          <Route path="/research" element={<Research />} />
          <Route path="/research-groups" element={<Research />} />
          <Route path="/publications" element={<Research />} />
          <Route path="/research-projects" element={<Research />} />

          <Route path="/research-innovation" element={<Research />} />

          <Route path="/faculty-staff" element={<About />} />
          <Route path="/faculty" element={<About />} />
          <Route path="/all-students" element={<About />} />
          <Route path="/all-alumni" element={<About />} />
          <Route path="/visitors" element={<About />} />

          <Route path="/all-news" element={<News />} />
          <Route path="/all-events" element={<Events />} />
          <Route path="/all-announcements" element={<Announcements />} />
          <Route path="/all-contacts" element={<Contacts />} />

          <Route path="*" element={<GenericNotFound />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}
