import { useRef } from 'react'
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
  const iframeRef = useRef(null)

  return (
    <div className="fullscreen-page" style={{
      display: 'flex',
      flexDirection: 'column',
      height: '100vh',
      overflow: 'hidden',
      backgroundColor: '#242424',
      position: 'relative'
    }}>
      <iframe
        ref={iframeRef}
        src="/unity/index-render.html"
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
        onLoad={() => console.log('Unity game iframe loaded')}
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
