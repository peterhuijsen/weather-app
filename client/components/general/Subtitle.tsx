/*
Used for describing a consistent subtitle text across the application.
 */
export default function Subtitle({ children, className, ...params } : any) {
	return (
		<h2 className={`text-neutral-900 text-xl font-medium ${className}`} {...params}>
			{children}
		</h2>
	)
}