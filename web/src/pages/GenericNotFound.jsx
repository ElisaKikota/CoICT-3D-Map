import { Link } from 'react-router-dom'
import SimplePage from './SimplePage.jsx'

export default function GenericNotFound() {
  return (
    <SimplePage title="Page Not Found">
      <p>The page you requested does not exist.</p>
      <p>
        Go back to <Link className="text-blue-600 underline" to="/">Home</Link>.
      </p>
    </SimplePage>
  )
}
