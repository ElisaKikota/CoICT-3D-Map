export default function Footer() {
  return (
    <footer className="bg-[#2B579A] text-white">
      <div className="container mx-auto px-4 py-8">
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-8">
          <div>
            <h3 className="text-lg font-semibold mb-3 border-b border-blue-400 pb-2">CONTACTS</h3>
            <div className="space-y-2 text-sm">
              <p>College of Information and Communication Technologies</p>
              <p>University of Dar es Salaam</p>
            </div>
          </div>

          <div>
            <h3 className="text-lg font-semibold mb-3 border-b border-blue-400 pb-2">COLLEGE UNITS</h3>
            <ul className="space-y-2 text-sm">
              <li><a className="hover:text-gray-200 transition-colors duration-200" href="#">Unit 1</a></li>
              <li><a className="hover:text-gray-200 transition-colors duration-200" href="#">Unit 2</a></li>
            </ul>
          </div>

          <div>
            <h3 className="text-lg font-semibold mb-3 border-b border-blue-400 pb-2">RESEARCH GROUPS</h3>
            <ul className="space-y-2 text-sm">
              <li><a className="hover:text-gray-200 transition-colors duration-200" href="#">Group 1</a></li>
              <li><a className="hover:text-gray-200 transition-colors duration-200" href="#">Group 2</a></li>
            </ul>
          </div>

          <div>
            <h3 className="text-lg font-semibold mb-3 border-b border-blue-400 pb-2">SOCIAL</h3>
            <div className="text-sm">
              <p>Add your social links here.</p>
            </div>
          </div>
        </div>

        <div className="mt-8 pt-6 border-t border-blue-400">
          <p className="text-xs sm:text-sm text-center">
            © {new Date().getFullYear()} College of Information and Communication Technologies. All Rights Reserved.
          </p>
        </div>
      </div>
    </footer>
  )
}
