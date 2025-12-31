import { useState, useEffect, useRef } from 'react'
import './App.css'

function App() {
  const [gameLoaded, setGameLoaded] = useState(false)
  const [showFullscreenPrompt, setShowFullscreenPrompt] = useState(false)
  const [loadingProgress, setLoadingProgress] = useState(0)
  const [isFullscreen, setIsFullscreen] = useState(false)
  const iframeRef = useRef(null)

  // Preload Unity build files and track progress
  useEffect(() => {
    const preloadFiles = async () => {
      const filesToPreload = [
        '/unity/Build/CoICT3DMap.loader.js',
        '/unity/Build/CoICT3DMap.data.unityweb',
        '/unity/Build/CoICT3DMap.framework.js.unityweb',
        '/unity/Build/CoICT3DMap.wasm.unityweb'
      ]

      let loadedCount = 0
      const totalFiles = filesToPreload.length

      const loadFile = async (url) => {
        try {
          const response = await fetch(url, { method: 'HEAD' })
          if (response.ok) {
            loadedCount++
            setLoadingProgress((loadedCount / totalFiles) * 100)
          }
        } catch (error) {
          console.warn(`Failed to preload ${url}:`, error)
          // Still count as loaded if it exists (network might be slow)
          loadedCount++
          setLoadingProgress((loadedCount / totalFiles) * 100)
        }
      }

      // Load files in parallel
      await Promise.all(filesToPreload.map(loadFile))
      
      // Set progress to 100% and wait a bit before showing game
      setLoadingProgress(100)
      setTimeout(() => {
        setGameLoaded(true)
      }, 500)
    }

    preloadFiles()
  }, [])

  // Handle iframe load event
  useEffect(() => {
    if (!gameLoaded) return

    const iframe = iframeRef.current
    if (!iframe) return

    const handleIframeLoad = () => {
      // Iframe loaded, game should be starting
      console.log('Unity game iframe loaded')
    }

    const handleIframeError = (error) => {
      console.error('Iframe error:', error)
      // Show error message to user
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
  }, [gameLoaded])

  // Check resolution and show fullscreen prompt
  useEffect(() => {
    if (!gameLoaded) return

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
  }, [gameLoaded, showFullscreenPrompt])

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

  if (!gameLoaded) {
    return (
      <div className="loading-screen">
        <div className="loading-content">
          <div className="loading-spinner"></div>
          <h2>Downloading Game...</h2>
          <div className="progress-bar-container">
            <div 
              className="progress-bar" 
              style={{ width: `${loadingProgress}%` }}
            ></div>
          </div>
          <p>{Math.round(loadingProgress)}%</p>
          <p className="loading-hint">Please wait while we prepare your game experience...</p>
        </div>
      </div>
    )
  }

  return (
    <div style={{
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
            <p className="current-resolution">
              Current: {window.innerWidth} x {window.innerHeight}
            </p>
            <div className="fullscreen-buttons">
              <button 
                onClick={handleFullscreen}
                className="fullscreen-button primary"
              >
                Enter Fullscreen
              </button>
              <button 
                onClick={handleDismissPrompt}
                className="fullscreen-button secondary"
              >
                Continue Anyway
              </button>
            </div>
          </div>
        </div>
      )}
      
      <div style={{
        flex: 1,
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        overflow: 'hidden',
        width: '100%',
        height: '100%'
      }}>
        <iframe
          ref={iframeRef}
          title="CoICT 3D Map"
          src="/unity/index.html"
          style={{
            width: '100vw',
            height: '100vh',
            maxWidth: '100%',
            maxHeight: '100%',
            aspectRatio: '1080 / 1920',
            objectFit: 'contain',
            border: 'none',
            display: 'block'
          }}
          allow="fullscreen; autoplay; camera; microphone; gamepad"
          allowFullScreen
          sandbox="allow-same-origin allow-scripts allow-forms allow-popups allow-popups-to-escape-sandbox allow-presentation"
        />
      </div>
    </div>
  )
}

export default App
