import { NavLink, useParams } from 'react-router-dom'

const aboutNav = [
  { slug: 'background', label: 'Background' },
  { slug: 'vision-mission', label: 'Vision, mission & core values' },
  { slug: 'Strength-opportunities', label: 'Strength & opportunities' },
  { slug: 'Infrastructure-facilities', label: 'Infrastructure & facilities' },
  { slug: 'campus-life', label: 'Campus life' },
  { slug: 'campus-virtual-tour', label: '2D/3D Campus Tour', isTour: true },
  { slug: 'governance', label: 'Governance' },
  { slug: 'fact-sheets', label: 'Fact sheets' },
]

function pageTitle(slug) {
  const item = aboutNav.find((x) => x.slug === slug)
  return item?.label ?? 'About'
}

function pageBody(slug) {
  if (slug === 'background') {
    return (
      <div className="space-y-3">
        <p>
          The College of Information and Communication Technologies (CoICT) is among the colleges of the University of Dar es Salaam (UDSM) in Tanzania.
        </p>
        <p>
          It was established on 27th January, 2011 to address the increasing demand of ICT experts in the country by strengthening the University's teaching,
          research and public service in the ICT-related disciplines.
        </p>
      </div>
    )
  }

  if (slug === 'vision-mission') {
    return (
      <div className="space-y-3">
        <p>Vision, mission and core values content goes here.</p>
      </div>
    )
  }

  return (
    <div className="space-y-3">
      <p>{pageTitle(slug)} content goes here.</p>
    </div>
  )
}

export default function About() {
  const { page } = useParams()
  const slug = page || 'background'

  return (
    <div className="bg-gray-100 py-10">
      <div className="test-css">
        CSS test: if this is green, CSS is loading.
      </div>
      <div className="container mx-auto px-4">
        <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
          <aside className="bg-white rounded-xl shadow p-4 lg:col-span-1">
            <div className="text-gray-900 font-semibold mb-3">About</div>
            <nav className="flex flex-col">
              {aboutNav.map((item) => {
                const to = item.isTour ? '/virtual-tours' : `/about/${item.slug}`

                return (
                  <NavLink
                    key={item.slug}
                    to={to}
                    className={({ isActive }) =>
                      `px-3 py-2 rounded-lg text-sm transition-colors ${
                        isActive && !item.isTour ? 'bg-blue-50 text-blue-800' : 'text-gray-700 hover:bg-gray-50'
                      }`
                    }
                    end
                  >
                    {item.label}
                  </NavLink>
                )
              })}
            </nav>
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
