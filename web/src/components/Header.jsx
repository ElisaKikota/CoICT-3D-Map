import { Link, NavLink } from 'react-router-dom'

export default function Header() {
  const linkClass = ({ isActive }) =>
    `px-3 lg:px-4 py-3 lg:py-4 hover:text-blue-700 transition-colors duration-200 ${
      isActive ? 'text-blue-700 border-b-2 border-blue-700' : 'text-[#2B579A]'
    }`

  return (
    <div className="w-full">
      <div className="bg-[#2B579A] text-white w-full">
        <div className="hidden lg:block py-2">
          <div className="container mx-auto px-4">
            <div className="flex justify-end lg:justify-center space-x-4 text-sm">
              <NavLink to="/all-students" className="hover:text-gray-200 transition-colors duration-200">Students</NavLink>
              <NavLink to="/faculty-staff" className="hover:text-gray-200 transition-colors duration-200">Staffs</NavLink>
              <NavLink to="/faculty" className="hover:text-gray-200 transition-colors duration-200">Our College</NavLink>
              <NavLink to="/all-alumni" className="hover:text-gray-200 transition-colors duration-200">Alumni</NavLink>
              <NavLink to="/visitors" className="hover:text-gray-200 transition-colors duration-200">Visitors</NavLink>
            </div>
          </div>
        </div>

        <div className="border-b border-blue-400">
          <div className="container mx-auto px-4">
            <div className="flex items-center justify-between py-2 sm:py-3 lg:py-4">
              <div className="w-16 h-16 sm:w-28 sm:h-28 md:w-32 md:h-32 lg:w-36 lg:h-36 flex-shrink-0 flex items-center justify-center">
                <img src="/tanzania.png" alt="Tanzania Logo" className="w-full h-full object-contain" />
              </div>

              <div className="flex-1 text-center px-4">
                <div className="hidden sm:block">
                  <h1 className="text-lg sm:text-xl md:text-2xl lg:text-3xl font-bold tracking-wide leading-tight">
                    <span className="block sm:hidden">COICT</span>
                    <span className="hidden sm:block md:hidden">College of ICT</span>
                    <span className="hidden md:block">COLLEGE OF INFORMATION AND COMMUNICATION TECHNOLOGIES</span>
                  </h1>
                  <p className="text-sm sm:text-base md:text-lg lg:text-xl mt-1">UNIVERSITY OF DAR ES SALAAM</p>
                </div>
              </div>

              <div className="hidden md:block w-24 h-24 sm:w-28 sm:h-28 md:w-32 md:h-32 lg:w-36 lg:h-36 flex-shrink-0 flex items-center justify-center">
                <img src="/udsmlogo.png" alt="UDSM Logo" className="w-full h-full object-contain" />
              </div>

              <div className="sm:hidden text-center">
                <Link to="/" className="font-bold">COICT</Link>
              </div>
            </div>
          </div>
        </div>
      </div>

      <nav className="bg-white text-[#2B579A] border-b border-gray-200">
        <div className="container mx-auto px-4">
          <div className="hidden lg:flex justify-center space-x-1">
            <NavLink to="/" className={linkClass} end>Home</NavLink>
            <NavLink to="/about/background" className={linkClass}>About</NavLink>
            <NavLink to="/academics/undergraduate" className={linkClass}>Academics</NavLink>
            <NavLink to="/academics/short-courses" className={linkClass}>Short Courses</NavLink>
            <NavLink to="/research" className={linkClass}>Research</NavLink>
            <NavLink to="/all-events" className={linkClass}>Events</NavLink>
            <NavLink to="/all-contacts" className={linkClass}>Contacts</NavLink>
            <NavLink to="/virtual-tours" className={linkClass}>2D/3D Campus Tour</NavLink>
          </div>

          <div className="lg:hidden flex flex-wrap gap-2 py-3 text-sm">
            <NavLink to="/" className="text-[#2B579A] underline">Home</NavLink>
            <NavLink to="/about/background" className="text-[#2B579A] underline">About</NavLink>
            <NavLink to="/academics/undergraduate" className="text-[#2B579A] underline">Academics</NavLink>
            <NavLink to="/virtual-tours" className="text-[#2B579A] underline">2D/3D Tour</NavLink>
          </div>
        </div>
      </nav>
    </div>
  )
}
