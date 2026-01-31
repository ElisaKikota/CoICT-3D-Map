import { Link } from 'react-router-dom'

export default function Home() {
  return (
    <div className="bg-gray-100">
      <section className="relative">
        <div
          className="h-[420px] sm:h-[520px] w-full bg-cover bg-center"
          style={{ backgroundImage: "url('/banner/coict-building.jpg')" }}
        >
          <div className="absolute inset-0 bg-black/40" />
          <div className="absolute inset-0 flex items-center">
            <div className="container mx-auto px-4">
              <h1 className="text-white text-3xl sm:text-5xl font-bold leading-tight max-w-4xl">
                Welcome to the College of Information and Communication Technologies
              </h1>
              <p className="text-white/90 mt-4 max-w-3xl text-base sm:text-lg">
                Empowering the next generation of technology leaders through excellence in education, research, and innovation.
              </p>
              <div className="mt-8 flex flex-wrap gap-3">
                <Link
                  to="/academics/undergraduate"
                  className="inline-flex items-center px-5 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
                >
                  Explore Programs
                </Link>
                <a
                  href="https://admission.udsm.ac.tz"
                  className="inline-flex items-center px-5 py-3 bg-white/90 text-gray-900 rounded-lg hover:bg-white transition-colors"
                >
                  Apply Now
                </a>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section className="py-10">
        <div className="container mx-auto px-4 grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div className="bg-white rounded-xl shadow p-6">
            <h2 className="text-xl font-bold text-gray-900 mb-4">Message from Principal</h2>
            <div className="flex items-start gap-4">
              <div className="w-16 h-16 rounded-full bg-gray-100 flex items-center justify-center text-gray-700 font-bold">
                PJM
              </div>
              <div>
                <a
                  className="text-blue-700 font-semibold hover:underline"
                  href="https://coict.udsm.ac.tz/faculty-staff/3457c497-f41d-407d-9b54-700163ed876d"
                >
                  Prof. Baraka J. Maiseli
                </a>
                <p className="text-gray-600 text-sm">Principal</p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-xl shadow p-6 lg:col-span-2">
            <h2 className="text-xl font-bold text-gray-900 mb-4">About Our College</h2>
            <div className="text-gray-700 leading-relaxed space-y-3">
              <p>
                The College of Information and Communication Technologies (CoICT) is among the colleges of the University of Dar es Salaam (UDSM) in Tanzania.
                It was established on 27th January, 2011 to address the increasing demand of ICT experts in the country by strengthening the University's teaching,
                research and public service in the ICT-related disciplines through a more efficient utilization of the available human and physical resources.
              </p>
              <p>
                The College offers undergraduate and postgraduate degree programs that stress interactions between lecturers and students who are committed to
                academic excellence.
              </p>
              <div className="pt-2">
                <Link to="/about/background" className="text-blue-700 font-semibold hover:underline">Read more</Link>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section className="pb-10">
        <div className="container mx-auto px-4">
          <div className="flex items-end justify-between mb-4">
            <h2 className="text-2xl font-bold text-gray-900">College Management</h2>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
            <a
              href="https://coict.udsm.ac.tz/faculty-staff/3457c497-f41d-407d-9b54-700163ed876d"
              className="bg-white rounded-xl shadow p-5 hover:shadow-lg transition-shadow"
            >
              <div className="text-gray-900 font-semibold">Prof. Baraka J. Maiseli</div>
              <div className="text-gray-600 text-sm mt-1">Principal</div>
              <div className="text-gray-700 text-sm mt-3 line-clamp-3">
                Neural networks; machine vision; image &amp; video processing; AI &amp; robotics; mechatronics; embedded electronics.
              </div>
              <div className="text-blue-700 text-sm font-semibold mt-4">View Profile</div>
            </a>

            <a
              href="https://coict.udsm.ac.tz/faculty-staff/46696311-919a-47b6-97a5-4d1043e169ad"
              className="bg-white rounded-xl shadow p-5 hover:shadow-lg transition-shadow"
            >
              <div className="text-gray-900 font-semibold">Dr. Godwin M. Gadiel</div>
              <div className="text-gray-600 text-sm mt-1">Head of Department</div>
              <div className="text-gray-700 text-sm mt-3 line-clamp-3">
                Wireless Communications; Massive MIMO; mmWave and Terahertz communication systems; signal processing
              </div>
              <div className="text-blue-700 text-sm font-semibold mt-4">View Profile</div>
            </a>

            <a
              href="https://coict.udsm.ac.tz/faculty-staff/00e2705f-f719-409d-832b-be5cc39c8e15"
              className="bg-white rounded-xl shadow p-5 hover:shadow-lg transition-shadow"
            >
              <div className="text-gray-900 font-semibold">Dr. Mercy E. Mbise</div>
              <div className="text-gray-600 text-sm mt-1">Head of Department</div>
              <div className="text-gray-700 text-sm mt-3 line-clamp-3">
                Multimedia communication; computer networks; Image and video coding; E-Learning; geospatial and ICT technologies
              </div>
              <div className="text-blue-700 text-sm font-semibold mt-4">View Profile</div>
            </a>

            <a
              href="https://coict.udsm.ac.tz/faculty-staff/260da913-a94a-4990-b8b4-9d4a0839432c"
              className="bg-white rounded-xl shadow p-5 hover:shadow-lg transition-shadow"
            >
              <div className="text-gray-900 font-semibold">Dr. Fatuma Simba</div>
              <div className="text-gray-600 text-sm mt-1">Director</div>
              <div className="text-gray-700 text-sm mt-3 line-clamp-3">Database Systems</div>
              <div className="text-blue-700 text-sm font-semibold mt-4">View Profile</div>
            </a>
          </div>
        </div>
      </section>

      <section className="py-10 bg-white">
        <div className="container mx-auto px-4 grid grid-cols-1 lg:grid-cols-2 gap-8">
          <div>
            <div className="flex items-end justify-between mb-4">
              <h2 className="text-2xl font-bold text-gray-900">Latest Announcements</h2>
              <Link to="/all-announcements" className="text-blue-700 font-semibold hover:underline">View All</Link>
            </div>
            <div className="space-y-4">
              <a
                href="https://coict.udsm.ac.tz/announcements/workshop-on-artificial-intelligence-and-machine-learning"
                className="block bg-gray-50 border rounded-xl p-5 hover:bg-gray-100 transition-colors"
              >
                <div className="text-gray-500 text-sm">May 19, 2025 • Workshop</div>
                <div className="text-gray-900 font-semibold mt-1">Workshop on Artificial Intelligence and Machine Learning</div>
                <div className="text-gray-700 text-sm mt-2 line-clamp-2">
                  A comprehensive workshop on AI and ML will be conducted next month. Industry experts will share their knowledge and prac...
                </div>
                <div className="text-blue-700 text-sm font-semibold mt-3">Read More</div>
              </a>
            </div>
          </div>

          <div>
            <div className="flex items-end justify-between mb-4">
              <h2 className="text-2xl font-bold text-gray-900">Featured Programs</h2>
              <Link to="/academics/undergraduate" className="text-blue-700 font-semibold hover:underline">View All Programs</Link>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <a
                href="https://coict.udsm.ac.tz/academics/department/ete/program/f7ff7aa8-749a-429b-8c00-b75f40bf252e"
                className="block bg-gray-50 border rounded-xl p-5 hover:bg-gray-100 transition-colors"
              >
                <div className="text-gray-900 font-semibold">BSc Telecommunications Engineering</div>
                <div className="text-gray-600 text-sm mt-2">Department: Department of Electronics &amp; Telecommunications Engineering</div>
                <div className="text-gray-600 text-sm">Level: Undergraduate</div>
                <div className="text-blue-700 text-sm font-semibold mt-3">Learn More</div>
              </a>

              <a
                href="https://coict.udsm.ac.tz/academics/department/cvl/program/015ca863-d99f-4492-866d-c090014929cc"
                className="block bg-gray-50 border rounded-xl p-5 hover:bg-gray-100 transition-colors"
              >
                <div className="text-gray-900 font-semibold">Masters of Engineering Management</div>
                <div className="text-gray-600 text-sm mt-2">Level: Postgraduate</div>
                <div className="text-blue-700 text-sm font-semibold mt-3">Learn More</div>
              </a>

              <a
                href="https://coict.udsm.ac.tz/academics/department/cse/program/86b1188a-4c5b-4c34-9815-e1b62053945e"
                className="block bg-gray-50 border rounded-xl p-5 hover:bg-gray-100 transition-colors sm:col-span-2"
              >
                <div className="text-gray-900 font-semibold">BSc in Business Information Technology (BSc BIT)</div>
                <div className="text-gray-600 text-sm mt-2">Department: Department of Computer Science &amp; Engineering</div>
                <div className="text-gray-600 text-sm">Level: Undergraduate</div>
                <div className="text-blue-700 text-sm font-semibold mt-3">Learn More</div>
              </a>
            </div>
          </div>
        </div>
      </section>

      <section className="py-10">
        <div className="container mx-auto px-4 grid grid-cols-1 lg:grid-cols-2 gap-8">
          <div>
            <div className="flex items-end justify-between mb-4">
              <h2 className="text-2xl font-bold text-gray-900">Latest News</h2>
              <Link to="/all-news" className="text-blue-700 font-semibold hover:underline">View All News</Link>
            </div>
            <div className="space-y-4">
              <a
                href="https://coict.udsm.ac.tz/news/congratulation-prof-abdi-t-abdalla-for-your-appointment-as-editor-in-chief-of-the-journal-of-ict-systems-hosted-under-the-college-of-information-and-communication-technologies-university-of-dar-es-salaam"
                className="block bg-white rounded-xl shadow p-5 hover:shadow-lg transition-shadow"
              >
                <div className="text-gray-900 font-semibold">
                  Congratulation Prof. Abdi T. Abdalla for your appointment as Editor-in-Chief of the Journal of ICT Systems, hosted under the College of Information and Communication Technologies, University of Dar es Salaam.
                </div>
                <div className="text-blue-700 text-sm font-semibold mt-3">Read More</div>
              </a>

              <a
                href="https://coict.udsm.ac.tz/news/university-of-dar-es-salaam-announces-20242025-graduation-ceremonies"
                className="block bg-white rounded-xl shadow p-5 hover:shadow-lg transition-shadow"
              >
                <div className="text-gray-900 font-semibold">University of Dar es Salaam Announces 2024/2025 Graduation Ceremonies</div>
                <div className="text-gray-700 text-sm mt-2 line-clamp-2">
                  Dar es Salaam, July 16, 2025 - The University of Dar es Salaam (UDSM) has officially released the amended graduation dates for the 2024/2025 academic year...
                </div>
                <div className="text-blue-700 text-sm font-semibold mt-3">Read More</div>
              </a>
            </div>
          </div>

          <div>
            <div className="flex items-end justify-between mb-4">
              <h2 className="text-2xl font-bold text-gray-900">Contact Us</h2>
              <Link to="/all-contacts" className="text-blue-700 font-semibold hover:underline">Contacts</Link>
            </div>
            <div className="bg-white rounded-xl shadow p-6">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
                <div>
                  <h3 className="font-semibold text-gray-900">Visit Us</h3>
                  <p className="text-gray-700 text-sm mt-2">College of Information and Communication Technologies (CoICT)</p>
                  <p className="text-gray-700 text-sm">University of Dar es Salaam</p>
                  <p className="text-gray-700 text-sm">P.O. Box 33335, Dar es Salaam, Tanzania</p>
                </div>
                <div>
                  <h3 className="font-semibold text-gray-900">Email Us</h3>
                  <a className="text-blue-700 text-sm hover:underline mt-2 inline-block" href="mailto:coict@udsm.ac.tz">coict@udsm.ac.tz</a>

                  <h3 className="font-semibold text-gray-900 mt-4">Call Us</h3>
                  <p className="text-gray-700 text-sm mt-2">+255 222 410 645</p>
                  <p className="text-gray-700 text-sm">+255 222 410 194</p>
                </div>
              </div>

              <div className="mt-6 border-t pt-5">
                <h3 className="font-semibold text-gray-900">Office Hours</h3>
                <p className="text-gray-700 text-sm mt-2">Monday - Friday: 8:00 AM - 4:00 PM</p>
                <p className="text-gray-700 text-sm">Saturday - Sunday: Closed</p>
                <a
                  className="text-blue-700 text-sm font-semibold hover:underline mt-3 inline-block"
                  href="https://www.google.com/maps/dir//College+of+Information+and+Communication+Technologies+(CoICT)+-+University+of+Dar+es+salaam/@-6.7714281,39.2373848,17z/"
                >
                  Get Directions
                </a>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section className="pb-12">
        <div className="container mx-auto px-4">
          <div className="bg-[#2B579A] text-white rounded-2xl p-6 sm:p-10 flex flex-col md:flex-row items-start md:items-center justify-between gap-6">
            <div>
              <h2 className="text-2xl font-bold">2D/3D Campus Tour</h2>
              <p className="text-white/90 mt-2">
                Open the CoICT interactive campus tour.
              </p>
            </div>
            <Link
              to="/virtual-tours"
              className="inline-flex items-center px-6 py-3 bg-white text-[#2B579A] rounded-lg font-semibold hover:bg-gray-100 transition-colors"
            >
              Open Tour
            </Link>
          </div>
        </div>
      </section>
    </div>
  )
}
