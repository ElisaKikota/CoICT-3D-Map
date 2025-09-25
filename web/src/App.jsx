import { useState, useEffect } from 'react'
import './App.css'

function App() {
  const [dimensions, setDimensions] = useState({ width: 0, height: 0 })
  const [isLandscape, setIsLandscape] = useState(false)

  useEffect(() => {
    const updateDimensions = () => {
      const width = window.innerWidth
      const height = window.innerHeight
      const landscape = width > height
      
      setDimensions({ width, height })
      setIsLandscape(landscape)
    }

    updateDimensions()
    window.addEventListener('resize', updateDimensions)
    window.addEventListener('orientationchange', updateDimensions)
    
    return () => {
      window.removeEventListener('resize', updateDimensions)
      window.removeEventListener('orientationchange', updateDimensions)
    }
  }, [])

  const getGameDimensions = () => {
    if (isLandscape) {
      // Landscape: full screen for optimal gaming
      return {
        width: dimensions.width,
        height: dimensions.height,
        containerStyle: { width: '100%', height: '100vh' }
      }
    } else {
      // Portrait: full width, height minus small header
      return {
        width: dimensions.width,
        height: dimensions.height - 60, // Small header space
        containerStyle: { width: '100%', height: `${dimensions.height - 60}px` }
      }
    }
  }

  const gameConfig = getGameDimensions()

  return (
    <div style={{
      display: 'flex',
      flexDirection: 'column',
      height: '100vh',
      overflow: 'hidden',
      backgroundColor: '#242424'
    }}>
      {!isLandscape && (
        <div style={{
          padding: '8px',
          backgroundColor: '#242424',
          borderBottom: '1px solid #333',
          flexShrink: 0
        }}>
          <h1 style={{
            margin: 0,
            fontSize: '1.2rem',
            color: 'white',
            textAlign: 'center'
          }}>CoICT 3D Map</h1>
        </div>
      )}
      
      <div style={{
        flex: 1,
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        overflow: 'hidden'
      }}>
        <iframe
          title="CoICT 3D Map"
          src="/unity/index.html"
          style={{
            width: gameConfig.containerStyle.width,
            height: gameConfig.containerStyle.height,
            border: 'none',
            display: 'block'
          }}
          allowFullScreen
          loading="eager"
        />
      </div>
    </div>
  )
}

export default App
