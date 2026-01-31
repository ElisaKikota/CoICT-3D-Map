import { NavLink, useParams } from 'react-router-dom'

const academicsNav = [
  { slug: 'short-courses', label: 'Non-Degree' },
  { slug: 'undergraduate', label: 'Undergraduate' },
  { slug: 'postgraduate', label: 'Postgraduate' },
]

function pageTitle(slug) {
  const item = academicsNav.find((x) => x.slug === slug)
  return item?.label ?? 'Academics'
}

function pageBody(slug) {
  if (slug === 'undergraduate') {
    return (
      <div className="space-y-3">
        <p>Undergraduate programs list goes here.</p>
      </div>
    )
  }

  if (slug === 'postgraduate') {
    return (
      <div className="space-y-3">
        <p>Postgraduate programs list goes here.</p>
      </div>
    )
  }

  if (slug === 'short-courses') {
    return (
      <div className="space-y-3">
        <p>Non-degree and short courses list goes here.</p>
      </div>
    )
  }

  return (
    <div className="space-y-3">
      <p>{pageTitle(slug)} content goes here.</p>
    </div>
  )
}

export default function Academics() {
  const { category } = useParams()
  const slug = category || 'undergraduate'

  return (
    <div className="bg-gray-100 py-10">
      <div className="container mx-auto px-4">
        <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
          <aside className="bg-white rounded-xl shadow p-4 lg:col-span-1">
            <div className="text-gray-900 font-semibold mb-3">Academics</div>
            <nav className="flex flex-col">
              {academicsNav.map((item) => (
                <NavLink
                  key={item.slug}
                  to={`/academics/${item.slug}`}
                  className={({ isActive }) =>
                    `px-3 py-2 rounded-lg text-sm transition-colors ${
                      isActive ? 'bg-blue-50 text-blue-800' : 'text-gray-700 hover:bg-gray-50'
                    }`
                  }
                  end
                >
                  {item.label}
                </NavLink>
              ))}
            </nav>

            <div className="mt-6 pt-5 border-t">
              <div className="text-gray-900 font-semibold mb-2 text-sm">Departments</div>
              <div className="text-sm text-gray-700 space-y-2">
                <a className="block hover:underline" href="https://coict.udsm.ac.tz/academics/department/cse">Department of Computer Science &amp; Engineering</a>
                <a className="block hover:underline" href="https://coict.udsm.ac.tz/academics/department/ete">Department of Electronics and Telecommunications Engineering</a>
                <a className="block hover:underline" href="https://coict.udsm.ac.tz/academics/center/cvl">Centre for Virtual Learning</a>
              </div>
            </div>
          </aside>

          <section className="bg-white rounded-xl shadow p-6 lg:col-span-3">
            <h1 className="text-2xl sm:text-3xl font-bold text-gray-900 mb-4">{pageTitle(slug)}</h1>
            <div className="text-gray-700 leading-relaxed">{pageBody(slug)}</div>
          </section>
        </div>
      </div>
    </div>
  )
}
