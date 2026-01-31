export default function SimplePage({ title, children }) {
  return (
    <div className="bg-gray-100 py-10">
      <div className="container mx-auto px-4">
        <div className="bg-white rounded-xl shadow p-6 sm:p-10">
          <h1 className="text-2xl sm:text-3xl font-bold text-gray-900 mb-4">{title}</h1>
          <div className="text-gray-700 leading-relaxed space-y-3">
            {children}
          </div>
        </div>
      </div>
    </div>
  )
}
